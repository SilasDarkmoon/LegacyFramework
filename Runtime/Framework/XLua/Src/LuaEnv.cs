/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if USE_UNI_LUA
using System.Collections;
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using System.Collections;
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

namespace XLua
{
    using Capstones.UnityEngineEx;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class LuaEnv : IDisposable
    {
        internal static string _OldPath = null;

        internal RealStatePtr rawL;

        internal RealStatePtr L
        {
            get
            {
                if (rawL == RealStatePtr.Zero)
                {
                    throw new InvalidOperationException("this lua env had disposed!");
                }
                return rawL;
            }
        }

        private LuaTable _G;

        internal ObjectTranslator translator;

        internal int errorFuncRef = -1;

#if THREAD_SAFE || HOTFIX_ENABLE
        internal object luaLock = new object();

        internal object luaEnvLock
        {
            get
            {
                return luaLock;
            }
        }
#endif

        const int LIB_VERSION_EXPECT = 104;

        public LuaEnv()
        {
            if (LuaAPI.xlua_get_lib_version() != LIB_VERSION_EXPECT)
            {
                throw new InvalidProgramException("wrong lib version expect:"
                    + LIB_VERSION_EXPECT + " but got:" + LuaAPI.xlua_get_lib_version());
            }

#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                LuaIndexes.LUA_REGISTRYINDEX = LuaAPI.xlua_get_registry_index();
#if GEN_CODE_MINIMIZE
                LuaAPI.xlua_set_csharp_wrapper_caller(InternalGlobals.CSharpWrapperCallerPtr);
#endif
                // Create State
                rawL = LuaAPI.luaL_newstate();

                //Init Base Libs
                LuaAPI.luaopen_xlua(rawL);
                LuaAPI.luaopen_i64lib(rawL);
                LuaAPI.luaopen_perflib(rawL);

                translator = new ObjectTranslator(this, rawL);
                translator.createFunctionMetatable(rawL);
                translator.OpenLib(rawL);
                ObjectTranslatorPool.Instance.Add(rawL, translator);

                LuaAPI.lua_atpanic(rawL, StaticLuaCallbacks.Panic);

#if !XLUA_GENERAL
                LuaAPI.lua_pushstdcallcfunction(rawL, StaticLuaCallbacks.Print);
                if (0 != LuaAPI.xlua_setglobal(rawL, "print"))
                {
                    throw new Exception("[Print] call xlua_setglobal fail!");
                }
                LuaAPI.lua_pushstdcallcfunction(rawL, StaticLuaCallbacks.GLogInfo);
                if (0 != LuaAPI.xlua_setglobal(rawL, "GLogInfo"))
                {
                    throw new Exception("[GLogLog] call xlua_setglobal fail!");
                }
                LuaAPI.lua_pushstdcallcfunction(rawL, StaticLuaCallbacks.GLogError);
                if (0 != LuaAPI.xlua_setglobal(rawL, "GLogError"))
                {
                    throw new Exception("[GLogError] call xlua_setglobal fail!");
                }
                LuaAPI.lua_pushstdcallcfunction(rawL, StaticLuaCallbacks.GLogWarning);
                if (0 != LuaAPI.xlua_setglobal(rawL, "GLogWarning"))
                {
                    throw new Exception("[GLogWarning] call xlua_setglobal fail!");
                }
#endif

                //template engine lib register
                TemplateEngine.LuaTemplate.OpenLib(rawL);

                AddSearcher(StaticLuaCallbacks.LoadBuiltinLib, 2); // just after the preload searcher
                AddSearcher(StaticLuaCallbacks.LoadFromCustomLoaders, 3);
#if !XLUA_GENERAL
                //AddSearcher(StaticLuaCallbacks.LoadFromResource, 4);
                //AddSearcher(StaticLuaCallbacks.LoadFromStreamingAssetsPath, -1);
#endif

                InjectCustomFunctions();

                DoString(init_xlua, "Init");
                init_xlua = null;

                AddBuildin("socket.core", StaticLuaCallbacks.LoadSocketCore);
                AddBuildin("socket", StaticLuaCallbacks.LoadSocketCore);
                AddBuildin("rapidjson", LuaDLL.Lua.LoadRapidJson);

                //AddBuildin("leveldb", LuaDLL.Lua.LoadLevelDB);

                LuaAPI.lua_newtable(rawL); //metatable of indexs and newindexs functions
                LuaAPI.xlua_pushasciistring(rawL, "__index");
                LuaAPI.lua_pushstdcallcfunction(rawL, StaticLuaCallbacks.MetaFuncIndex);
                LuaAPI.lua_rawset(rawL, -3);

                LuaAPI.xlua_pushasciistring(rawL, Utils.LuaIndexsFieldName);
                LuaAPI.lua_newtable(rawL);
                LuaAPI.lua_pushvalue(rawL, -3);
                LuaAPI.lua_setmetatable(rawL, -2);
                LuaAPI.lua_rawset(rawL, LuaIndexes.LUA_REGISTRYINDEX);

                LuaAPI.xlua_pushasciistring(rawL, Utils.LuaNewIndexsFieldName);
                LuaAPI.lua_newtable(rawL);
                LuaAPI.lua_pushvalue(rawL, -3);
                LuaAPI.lua_setmetatable(rawL, -2);
                LuaAPI.lua_rawset(rawL, LuaIndexes.LUA_REGISTRYINDEX);

                LuaAPI.xlua_pushasciistring(rawL, Utils.LuaClassIndexsFieldName);
                LuaAPI.lua_newtable(rawL);
                LuaAPI.lua_pushvalue(rawL, -3);
                LuaAPI.lua_setmetatable(rawL, -2);
                LuaAPI.lua_rawset(rawL, LuaIndexes.LUA_REGISTRYINDEX);

                LuaAPI.xlua_pushasciistring(rawL, Utils.LuaClassNewIndexsFieldName);
                LuaAPI.lua_newtable(rawL);
                LuaAPI.lua_pushvalue(rawL, -3);
                LuaAPI.lua_setmetatable(rawL, -2);
                LuaAPI.lua_rawset(rawL, LuaIndexes.LUA_REGISTRYINDEX);

                LuaAPI.lua_pop(rawL, 1); // pop metatable of indexs and newindexs functions

                LuaAPI.xlua_pushasciistring(rawL, "xlua_main_thread");
                LuaAPI.lua_pushthread(rawL);
                LuaAPI.lua_rawset(rawL, LuaIndexes.LUA_REGISTRYINDEX);
#if !XLUA_GENERAL && (!UNITY_WSA || UNITY_EDITOR)
                translator.Alias(typeof(Type), "System.MonoType");
#endif

                if (0 != LuaAPI.xlua_getglobal(rawL, "_G"))
                {
                    throw new Exception("call xlua_getglobal fail!");
                }
                translator.Get(rawL, -1, out _G);
                LuaAPI.lua_pop(rawL, 1);

                errorFuncRef = LuaAPI.get_error_func_ref(rawL);

                if (initers != null)
                {
                    for (int i = 0; i < initers.Count; i++)
                    {
                        initers[i](this, translator);
                    }
                }

                translator.CreateArrayMetatable(rawL);
                translator.CreateDelegateMetatable(rawL);
                translator.CreateEnumerablePairs(rawL);

                CSFuncResetLoaders(rawL);

                //XLuaExt.LuaEvent.Init(rawL);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        private static List<Action<LuaEnv, ObjectTranslator>> initers = null;

        public static void AddIniter(Action<LuaEnv, ObjectTranslator> initer)
        {
            if (initers == null)
            {
                initers = new List<Action<LuaEnv, ObjectTranslator>>();
            }
            initers.Add(initer);
        }

        public LuaTable Global
        {
            get
            {
                return _G;
            }
        }

        public T LoadString<T>(byte[] chunk, string chunkName = "chunk", LuaTable env = null)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                if (typeof(T) != typeof(LuaFunction) && !typeof(T).IsSubclassOf(typeof(Delegate)))
                {
                    throw new InvalidOperationException(typeof(T).Name + " is not a delegate type nor LuaFunction");
                }
                var _L = L;
                int oldTop = LuaAPI.lua_gettop(_L);

                if (LuaAPI.xluaL_loadbuffer(_L, chunk, chunk.Length, chunkName) != 0)
                    ThrowExceptionFromError(oldTop);

                if (env != null)
                {
                    env.push(_L);
                    LuaAPI.lua_setfenv(_L, -2);
                }

                T result = (T)translator.GetObject(_L, -1, typeof(T));
                LuaAPI.lua_settop(_L, oldTop);

                return result;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public T LoadString<T>(string chunk, string chunkName = "chunk", LuaTable env = null)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(chunk);
            return LoadString<T>(bytes, chunkName, env);
        }

        public LuaFunction LoadString(string chunk, string chunkName = "chunk", LuaTable env = null)
        {
            return LoadString<LuaFunction>(chunk, chunkName, env);
        }

        public object[] DoString(byte[] chunk, string chunkName = "chunk", LuaTable env = null)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                var _L = L;
                int oldTop = LuaAPI.lua_gettop(_L);
                int errFunc = LuaAPI.load_error_func(_L, errorFuncRef);
                if (LuaAPI.xluaL_loadbuffer(_L, chunk, chunk.Length, chunkName) == 0)
                {
                    if (env != null)
                    {
                        env.push(_L);
                        LuaAPI.lua_setfenv(_L, -2);
                    }

                    if (LuaAPI.lua_pcall(_L, 0, -1, errFunc) == 0)
                    {
                        LuaAPI.lua_remove(_L, errFunc);
                        return translator.popValues(_L, oldTop);
                    }
                    else
                        ThrowExceptionFromError(oldTop);
                }
                else
                    ThrowExceptionFromError(oldTop);

                return null;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public object[] DoString(string chunk, string chunkName = "chunk", LuaTable env = null)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(chunk);
            return DoString(bytes, chunkName, env);
        }

        private void AddSearcher(LuaCSFunction searcher, int index)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                var _L = L;
                //insert the loader
                LuaAPI.xlua_getloaders(_L);
                if (!LuaAPI.lua_istable(_L, -1))
                {
                    throw new Exception("Can not set searcher!");
                }
                uint len = LuaAPI.xlua_objlen(_L, -1);
                index = index < 0 ? (int)(len + index + 2) : index;
                for (int e = (int)len + 1; e > index; e--)
                {
                    LuaAPI.xlua_rawgeti(_L, -1, e - 1);
                    LuaAPI.xlua_rawseti(_L, -2, e);
                }
                LuaAPI.lua_pushstdcallcfunction(_L, searcher);
                LuaAPI.xlua_rawseti(_L, -2, index);
                LuaAPI.lua_pop(_L, 1);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public void Alias(Type type, string alias)
        {
            translator.Alias(type, alias);
        }

#if !XLUA_GENERAL
        int last_check_point = 0;

        int max_check_per_tick = 20;

        static bool ObjectValidCheck(object obj)
        {
            return (!(obj is UnityEngine.Object)) || ((obj as UnityEngine.Object) != null);
        }

        Func<object, bool> object_valid_checker = new Func<object, bool>(ObjectValidCheck);
#endif

        public void Tick()
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                var _L = L;
                lock (refQueue)
                {
                    while (refQueue.Count > 0)
                    {
                        GCAction gca = refQueue.Dequeue();
                        translator.ReleaseLuaBase(_L, gca.Reference, gca.IsDelegate);
                    }
                }
#if !XLUA_GENERAL
                last_check_point = translator.objects.Check(last_check_point, max_check_per_tick, object_valid_checker, translator.reverseMap);
#endif
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        //兼容API
        public void GC()
        {
            Tick();
        }

        public LuaTable NewTable()
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                var _L = L;
                int oldTop = LuaAPI.lua_gettop(_L);

                LuaAPI.lua_newtable(_L);
                LuaTable returnVal = (LuaTable)translator.GetObject(_L, -1, typeof(LuaTable));

                LuaAPI.lua_settop(_L, oldTop);
                return returnVal;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        private bool disposed = false;

        public void Dispose()
        {
            FullGc();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            Dispose(true);

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        public virtual void Dispose(bool dispose)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                if (disposed) return;
                Tick();

                if (!translator.AllDelegateBridgeReleased())
                {
                    throw new InvalidOperationException("try to dispose a LuaEnv with C# callback!");
                }

                ObjectTranslatorPool.Instance.Remove(L);

                LuaAPI.lua_close(L);
                translator = null;

                rawL = IntPtr.Zero;

                disposed = true;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public void ThrowExceptionFromError(int oldTop)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                object err = translator.GetObject(L, -1);
                LuaAPI.lua_settop(L, oldTop);
                string errString = err.ToString();

                // A pre-wrapped exception - just rethrow it (stack trace of InnerException will be preserved)
                Exception ex = err as Exception;
                if (ex != null)
                {
                    //BuglyAgent.PrintLog(LogSeverity.LogError, errString);
                    throw ex;
                }

                // A non-wrapped Lua error (best interpreted as a string) - wrap it and throw it
                if (err == null) errString = "Unknown Lua Error";
                //BuglyAgent.PrintLog(LogSeverity.LogError, errString);
                throw new LuaException(errString);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        internal struct GCAction
        {
            public int Reference;
            public bool IsDelegate;
        }

        Queue<GCAction> refQueue = new Queue<GCAction>();

        internal void equeueGCAction(GCAction action)
        {
            lock (refQueue)
            {
                refQueue.Enqueue(action);
            }
        }

        private string init_xlua = @" 
            local metatable = {}
            local rawget = rawget
            local setmetatable = setmetatable
            local import_type = xlua.import_type
            local load_assembly = xlua.load_assembly

            function metatable:__index(key) 
                local fqn = rawget(self,'.fqn')
                fqn = ((fqn and fqn .. '.') or '') .. key

                local obj = import_type(fqn)

                if obj == nil then
                    -- It might be an assembly, so we load it too.
                    obj = { ['.fqn'] = fqn }
                    setmetatable(obj, metatable)
                elseif obj == true then
                    return rawget(self, key)
                end

                -- Cache this lookup
                rawset(self, key, obj)
                return obj
            end

            -- A non-type has been called; e.g. foo = System.Foo()
            function metatable:__call(...)
                error('No such type: ' .. rawget(self,'.fqn'), 2)
            end

            CS = CS or {}
            setmetatable(CS, metatable)

            typeof = function(t) return t.UnderlyingSystemType end
            cast = xlua.cast
            if not setfenv or not getfenv then
                local function getfunction(level)
                    local info = debug.getinfo(level + 1, 'f')
                    return info and info.func
                end

                function setfenv(fn, env)
                  if type(fn) == 'number' then fn = getfunction(fn + 1) end
                  local i = 1
                  while true do
                    local name = debug.getupvalue(fn, i)
                    if name == '_ENV' then
                      debug.upvaluejoin(fn, i, (function()
                        return env
                      end), 1)
                      break
                    elseif not name then
                      break
                    end

                    i = i + 1
                  end

                  return fn
                end

                function getfenv(fn)
                  if type(fn) == 'number' then fn = getfunction(fn + 1) end
                  local i = 1
                  while true do
                    local name, val = debug.getupvalue(fn, i)
                    if name == '_ENV' then
                      return val
                    elseif not name then
                      break
                    end
                    i = i + 1
                  end
                end
            end

            xlua.hotfix = function(cs, field, func)
                if func == nil then func = false end
                local tbl = (type(field) == 'table') and field or {[field] = func}
                for k, v in pairs(tbl) do
                    local cflag = ''
                    if k == '.ctor' then
                        cflag = '_c'
                        k = 'ctor'
                    end
                    local f = type(v) == 'function' and v or nil
                    xlua.access(cs, cflag .. '__Hotfix0_'..k, f) -- at least one
                    pcall(function()
                        for i = 1, 99 do
                            xlua.access(cs, cflag .. '__Hotfix'..i..'_'..k, f)
                        end
                    end)
                end
                xlua.private_accessible(cs)
            end
            xlua.getmetatable = function(cs)
                return xlua.metatable_operation(cs)
            end
            xlua.setmetatable = function(cs, mt)
                return xlua.metatable_operation(cs, mt)
            end
            xlua.setclass = function(parent, name, impl)
                impl.UnderlyingSystemType = parent[name].UnderlyingSystemType
                rawset(parent, name, impl)
            end
            
            local base_mt = {
                __index = function(t, k)
                    local csobj = t['__csobj']
                    local func = csobj['<>xLuaBaseProxy_'..k]
                    return function(_, ...)
                         return func(csobj, ...)
                    end
                end
            }
            base = function(csobj)
                return setmetatable({__csobj = csobj}, base_mt)
            end
            ";

        public delegate byte[] CustomLoader(ref string filepath);

        internal List<CustomLoader> customLoaders = new List<CustomLoader>();

        //loader : CustomLoader， filepath参数：（ref类型）输入是require的参数，如果需要支持调试，需要输出真实路径。
        //                        返回值：如果返回null，代表加载该源下无合适的文件，否则返回UTF8编码的byte[]
        public void AddLoader(CustomLoader loader)
        {
            customLoaders.Add(loader);
        }

        internal Dictionary<string, LuaCSFunction> buildin_initer = new Dictionary<string, LuaCSFunction>();

        public void AddBuildin(string name, LuaCSFunction initer)
        {
            if (!Utils.IsStaticPInvokeCSFunction(initer))
            {
                throw new Exception("initer must be static and has MonoPInvokeCallback Attribute!");
            }
            buildin_initer.Add(name, initer);
        }

        //The garbage-collector pause controls how long the collector waits before starting a new cycle. 
        //Larger values make the collector less aggressive. Values smaller than 100 mean the collector 
        //will not wait to start a new cycle. A value of 200 means that the collector waits for the total 
        //memory in use to double before starting a new cycle.
        public int GcPause
        {
            get
            {
#if THREAD_SAFE || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    int val = LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETPAUSE, 200);
                    LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETPAUSE, val);
                    return val;
#if THREAD_SAFE || HOTFIX_ENABLE
                }
#endif
            }
            set
            {
#if THREAD_SAFE || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETPAUSE, value);
#if THREAD_SAFE || HOTFIX_ENABLE
                }
#endif
            }
        }

        //The step multiplier controls the relative speed of the collector relative to memory allocation. 
        //Larger values make the collector more aggressive but also increase the size of each incremental 
        //step. Values smaller than 100 make the collector too slow and can result in the collector never 
        //finishing a cycle. The default, 200, means that the collector runs at "twice" the speed of memory 
        //allocation.
        public int GcStepmul
        {
            get
            {
#if THREAD_SAFE || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    int val = LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETSTEPMUL, 200);
                    LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETSTEPMUL, val);
                    return val;
#if THREAD_SAFE || HOTFIX_ENABLE
                }
#endif
            }
            set
            {
#if THREAD_SAFE || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETSTEPMUL, value);
#if THREAD_SAFE || HOTFIX_ENABLE
                }
#endif
            }
        }

        public void FullGc()
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public void StopGc()
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSTOP, 0);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public void RestartGc()
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCRESTART, 0);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public bool GcStep(int data)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                return LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSTEP, data) != 0;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public int Memroy
        {
            get
            {
#if THREAD_SAFE || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    return LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCCOUNT, 0);
#if THREAD_SAFE || HOTFIX_ENABLE
                }
#endif
            }
        }

        #region Custom Function Injections

        void InjectCustomFunctions()
        {
            LuaAPI.lua_newtable(rawL);
            LuaAPI.lua_pushvalue(rawL, -1);
            LuaAPI.xlua_setglobal(rawL, "CS");
            LuaAPI.xlua_pushasciistring(rawL, "array");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncArray);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "dict");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncDict);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "table");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncTable);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "trans");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncGetLangValueOfUserDataType);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "transstr");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncGetLangValueOfStringType);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "as");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncConvert);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "is");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncIs);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "isnull");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncIsNull);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "encrypt");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncEncryptPostData);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "resp");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncParseResponse);
            LuaAPI.xlua_psettable(rawL, -3);
            //LuaAPI.xlua_pushasciistring(rawL, "platform");
            //LuaAPI.lua_pushstring(rawL, ThreadSafeValues.AppPlatform);
            //LuaAPI.xlua_psettable(rawL, -3);
            //LuaAPI.xlua_pushasciistring(rawL, "updatepath");
            //LuaAPI.lua_pushstring(rawL, Capstones.LuaExt.LuaFramework.AppUpdatePath);
            //LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "capid");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncGetCapID);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "toluastring");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncToLuaString);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "checkzip");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncCheckZipValid);
            LuaAPI.xlua_psettable(rawL, -3);
            LuaAPI.xlua_pushasciistring(rawL, "reset");
            LuaAPI.lua_pushstdcallcfunction(rawL, CSFuncReset);
            LuaAPI.xlua_psettable(rawL, -3);
        }

        private static string luaPackagePath
        {
            get
            {
#if UNITY_EDITOR
                return ThreadSafeValues.AppDataPath + "/CapstonesScripts/spt/xlua/?.lua";
#else
                return ThreadSafeValues.UpdatePath + "/spt/xlua/?.lua;" + ThreadSafeValues.AppStreamingAssetsPath + "/spt/xlua/?.lua";
#endif
            }
        }

        private static string GetLuaDistributePath(string flag)
        {
#if UNITY_EDITOR
            return ThreadSafeValues.AppDataPath + "/CapstonesScripts/distribute/" + flag + "/?.lua";
#else
            return ThreadSafeValues.UpdatePath + "/spt/distribute/" + flag + "/?.lua;" + ThreadSafeValues.AppStreamingAssetsPath + "/spt/distribute/" + flag + "/?.lua";
#endif
        }

        public int CSFuncResetLoaders(IntPtr l)
        {
            // Set package.path
            if (_OldPath == null)
            {
                _OldPath = this.Global.GetInPath<string>("package.path");
            }
            var packagePath = luaPackagePath + ";" + _OldPath;
            foreach (var flag in ResManager.GetDistributeFlags())
            {
                packagePath = GetLuaDistributePath(flag) + ";" + packagePath;
            }
            this.Global.SetInPath("package.path", packagePath);

#if UNITY_ANDROID && !UNITY_EDITOR
            //var package = this.Global.GetInPath<LuaTable>("package");
            //if (package != null)
            //{
            //    if (Capstones.LuaExt.LuaFramework.AppStreamingAssetsPath.Contains("://"))
            //    {
            //        if (ResManager.LoadAssetsFromApk)
            //        {
            //            var loaders = package.Get<LuaTable>("loaders");
            //            if (loaders != null)
            //            {
            //                loaders.Set<int, LuaCSFunction>(loaders.Length + 1, ClrFuncApkLoader);
            //            }
            //        }
            //    }
            //}

            if (ResManager.LoadAssetsFromApk)
            {
                LuaAPI.xlua_getglobal(l, "package");
                //l.GetGlobal("package"); // package
                GetField(l, -1, "loaders");
                //l.GetField(-2, "loaders"); // package loaders
                if (LuaAPI.lua_istable(l, -1))
                //if (l.istable(-1))
                {
                    GetField(l, -1, "apkloader");
                    //l.GetField(-1, "apkloader"); // package loaders apkloader
                    if (lua_isnoneornil(l, -1))
                    //if (l.isnoneornil(-1))
                    {
                        LuaAPI.lua_pop(l, 1);
                        //l.pop(1); // package loaders
                        LuaAPI.lua_pushstdcallcfunction(l, ClrFuncApkLoader);
                        //l.pushcfunction(ClrFuncApkLoader); // package loaders apkloader
                        LuaAPI.lua_pushvalue(l, -1);
                        //l.pushvalue(-1); // package loaders apkloader apkloader
                        SetField(l, -3, "apkloader");
                        //l.SetField(-3, "apkloader"); // package loaders apkloader 
                    }
                    LuaAPI.lua_pushnumber(l, 1);
                    //l.pushnumber(1); // package loaders apkloader 1
                    LuaAPI.xlua_pgettable(l, -3);
                    //l.gettable(-3); // package loaders apkloader 1stloader
                    if (LuaAPI.lua_rawequal(l, -1, -2) == 1)
                    //if (l.equal(-1, -2))
                    {
                        LuaAPI.lua_pop(l, 2);
                        //l.pop(2); // package loaders
                    }
                    else
                    {
                        LuaAPI.lua_pop(l, 1);
                        //l.pop(1); // package loaders apkloader
                        var cnt = (int)LuaAPI.xlua_objlen(l, -2);
                        //var cnt = l.getn(-2);
                        for (int i = cnt; i >= 1; --i)
                        {
                            LuaAPI.lua_pushnumber(l, i + 1);
                            //l.pushnumber(i + 1); // package loaders apkloader i+1
                            LuaAPI.lua_pushnumber(l, i);
                            //l.pushnumber(i); // package loaders apkloader i+1 i
                            LuaAPI.xlua_pgettable(l, -4);
                            //l.gettable(-4); // package loaders apkloader i+1 func
                            LuaAPI.xlua_psettable(l, -4);
                            //l.settable(-4); // package loaders apkloader
                        }
                        LuaAPI.lua_pushnumber(l, 1);
                        //l.pushnumber(1); // package loaders apkloader 1
                        LuaAPI.lua_insert(l, -2);
                        //l.insert(-2); // package loaders 1 apkloader
                        LuaAPI.xlua_psettable(l, -3);
                        //l.settable(-3); // package loaders
                    }
                }
                LuaAPI.lua_pop(l, 2);
                //l.pop(2); // X
            }
#endif

            return 0;
        }

        public static void GetField(IntPtr l, int index, string key)
        {
            var top = LuaAPI.lua_gettop(l); //l.gettop();
            if (index < 0 && -index <= top)
            {
                index = top + 1 + index;
            }

            LuaAPI.lua_pushstring(l, key);
            //l.PushString(key);
            LuaAPI.xlua_pgettable(l, index);
            //l.gettable(index);
        }

        public static void SetField(IntPtr l, int index, string key)
        {
            var top = LuaAPI.lua_gettop(l); //l.gettop();
            if (index < 0 && -index <= top)
            {
                index = top + 1 + index;
            }

            LuaAPI.lua_pushstring(l, key);
            //l.PushString(key);
            LuaAPI.lua_insert(l, -2);
            //l.insert(-2);
            LuaAPI.xlua_psettable(l, index);
            //l.settable(index);
        }

        public static bool lua_isnoneornil(IntPtr L, int index)
        {
            return (LuaAPI.lua_type(L, index) <= LuaTypes.LUA_TNIL);
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncReset(IntPtr l)
        {
            // old package.path
            if (_OldPath == null)
            {
                _OldPath = LuaBehaviour.luaEnv.Global.GetInPath<string>("package.path");
            }
            else
            {
                ResManager.UnloadAllRes();
                ResManager.ReloadDistributeFlags();
            }

            // Set package.path
            var packagePath = luaPackagePath + ";" + _OldPath;
            foreach (var flag in ResManager.GetDistributeFlags())
            {
                packagePath = GetLuaDistributePath(flag) + ";" + packagePath;
            }
            LuaBehaviour.luaEnv.Global.SetInPath("package.path", packagePath);

#if UNITY_ANDROID && !UNITY_EDITOR
            //var package = LuaBehaviour.luaEnv.Global.GetInPath<LuaTable>("package");
            //if (package != null)
            //{
            //    if (Capstones.LuaExt.LuaFramework.AppStreamingAssetsPath.Contains("://"))
            //    {
            //        if (ResManager.LoadAssetsFromApk)
            //        {
            //            var loaders = package.Get<LuaTable>("loaders");
            //            if (loaders != null)
            //            {
            //                loaders.Set<int, LuaCSFunction>(loaders.Length + 1, ClrFuncApkLoader);
            //            }
            //        }
            //    }
            //}

            if (ResManager.LoadAssetsFromApk)
            {
                LuaAPI.xlua_getglobal(l, "package");
                //l.GetGlobal("package"); // package
                GetField(l, -1, "loaders");
                //l.GetField(-1, "loaders"); // package loaders
                if (LuaAPI.lua_istable(l, -1))
                //if (l.istable(-1))
                {
                    GetField(l, -1, "apkloader");
                    //l.GetField(-1, "apkloader"); // package loaders apkloader
                    if (lua_isnoneornil(l, -1))
                    //if (l.isnoneornil(-1))
                    {
                        LuaAPI.lua_pop(l, 1);
                        //l.pop(1); // package loaders
                        LuaAPI.lua_pushstdcallcfunction(l, ClrFuncApkLoader);
                        //l.pushcfunction(ClrFuncApkLoader); // package loaders apkloader
                        LuaAPI.lua_pushvalue(l, -1);
                        //l.pushvalue(-1); // package loaders apkloader apkloader
                        SetField(l, -3, "apkloader");
                        //l.SetField(-3, "apkloader"); // package loaders apkloader 
                    }
                    LuaAPI.lua_pushnumber(l, 1);
                    //l.pushnumber(1); // package loaders apkloader 1
                    LuaAPI.xlua_pgettable(l, -3);
                    //l.gettable(-3); // package loaders apkloader 1stloader
                    if (LuaAPI.lua_rawequal(l, -1, -2) == 1)
                    //if (l.equal(-1, -2))
                    {
                        LuaAPI.lua_pop(l, 2);
                        //l.pop(2); // package loaders
                    }
                    else
                    {
                        LuaAPI.lua_pop(l, 1);
                        //l.pop(1); // package loaders apkloader
                        var cnt = (int)LuaAPI.xlua_objlen(l, -2);
                        //var cnt = l.getn(-2);
                        for (int i = cnt; i >= 1; --i)
                        {
                            LuaAPI.lua_pushnumber(l, i + 1);
                            //l.pushnumber(i + 1); // package loaders apkloader i+1
                            LuaAPI.lua_pushnumber(l, i);
                            //l.pushnumber(i); // package loaders apkloader i+1 i
                            LuaAPI.xlua_pgettable(l, -4);
                            //l.gettable(-4); // package loaders apkloader i+1 func
                            LuaAPI.xlua_psettable(l, -4);
                            //l.settable(-4); // package loaders apkloader
                        }
                        LuaAPI.lua_pushnumber(l, 1);
                        //l.pushnumber(1); // package loaders apkloader 1
                        LuaAPI.lua_insert(l, -2);
                        //l.insert(-2); // package loaders 1 apkloader
                        LuaAPI.xlua_psettable(l, -3);
                        //l.settable(-3); // package loaders
                    }
                }
                LuaAPI.lua_pop(l, 2);
                //l.pop(2); // X
            }
#endif

            // res version
            LuaBehaviour.luaEnv.Global.SetInPath<object>("exports.___resver", null);

            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int ClrFuncApkLoader(IntPtr L)
        {
            //string mname = LuaAPI.lua_tostring(L, 1);
            //if (!string.IsNullOrEmpty(mname))
            //{
            //    mname = mname.Replace('.', '/');

            //    var dflags = ResManager.GetDistributeFlags();
            //    for (int j = dflags.Length - 1; j >= 0; --j)
            //    {
            //        var flag = dflags[j];
            //        if (PlatDependant.IsFileExist(Capstones.LuaExt.LuaFramework.AppUpdatePath + "/spt/distribute/" + flag + "/" + mname + ".lua"))
            //        {
            //            return 0;
            //        }

            //        int retryTimes = 10;
            //        for (int i = 0; i < retryTimes; ++i)
            //        {
            //            System.Exception error = null;
            //            do
            //            {
            //                Unity.IO.Compression.ZipArchive za = ResManager.AndroidApkZipArchive;
            //                if (za == null)
            //                {
            //                    error = new Exception("Apk Archive Cannot be read.");
            //                    break;
            //                }
            //                try
            //                {
            //                    var entryname = "assets/spt/distribute/" + flag + "/" + mname + ".lua";
            //                    var entry = za.GetEntry(entryname);
            //                    if (entry != null)
            //                    {
            //                        var pathd = Capstones.LuaExt.LuaFramework.AppUpdatePath + "/spt/" + entryname.Substring("assets/spt/".Length);
            //                        using (var srcstream = entry.Open())
            //                        {
            //                            using (var dststream = Capstones.PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
            //                            {
            //                                srcstream.CopyTo(dststream);
            //                            }
            //                        }
            //                        PlatDependant.MoveFile(pathd + ".tmp", pathd);
            //                        return 0;
            //                    }
            //                }
            //                catch (Exception e)
            //                {
            //                    error = e;
            //                    break;
            //                }
            //            } while (false);
            //            if (error != null)
            //            {
            //                if (i == retryTimes - 1)
            //                {
            //                    if (GLog.IsLogErrorEnabled) GLog.LogException(error);
            //                    throw error;
            //                }
            //                else
            //                {
            //                    if (GLog.IsLogErrorEnabled) GLog.LogException(error + "\nNeed Retry " + i);
            //                }
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }
            //    }
            //    // none dflag
            //    {
            //        if (PlatDependant.IsFileExist(Capstones.LuaExt.LuaFramework.AppUpdatePath + "/spt/xlua/" + mname + ".lua"))
            //        {
            //            return 0;
            //        }

            //        int retryTimes = 10;
            //        for (int i = 0; i < retryTimes; ++i)
            //        {
            //            System.Exception error = null;
            //            do
            //            {
            //                Unity.IO.Compression.ZipArchive za = ResManager.AndroidApkZipArchive;
            //                if (za == null)
            //                {
            //                    error = new Exception("Apk Archive Cannot be read.");
            //                    break;
            //                }
            //                try
            //                {
            //                    var entryname = "assets/spt/xlua/" + mname + ".lua";
            //                    var entry = za.GetEntry(entryname);
            //                    if (entry != null)
            //                    {
            //                        var pathd = Capstones.LuaExt.LuaFramework.AppUpdatePath + "/spt/xlua/" + entryname.Substring("assets/spt/xlua/".Length);
            //                        using (var srcstream = entry.Open())
            //                        {
            //                            using (var dststream = Capstones.PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
            //                            {
            //                                srcstream.CopyTo(dststream);
            //                            }
            //                        }
            //                        PlatDependant.MoveFile(pathd + ".tmp", pathd);
            //                        return 0;
            //                    }
            //                }
            //                catch (Exception e)
            //                {
            //                    error = e;
            //                    break;
            //                }
            //            } while (false);
            //            if (error != null)
            //            {
            //                if (i == retryTimes - 1)
            //                {
            //                    if (GLog.IsLogErrorEnabled) GLog.LogException(error);
            //                    throw error;
            //                }
            //                else
            //                {
            //                    if (GLog.IsLogErrorEnabled) GLog.LogException(error + "\nNeed Retry " + i);
            //                }
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncArray(IntPtr L)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

            if (LuaAPI.lua_istable(L, 1))
            {
                var top = LuaAPI.lua_gettop(L);

                Array arr = null;
                var len = LuaAPI.xlua_objlen(L, 1);

                Type otype = (Type)translator.FastGetCSObj(L, 2);
                if (otype == null)
                {
                    otype = typeof(object);
                }
                arr = Array.CreateInstance(otype, len);
                for (int i = 0; i < len; ++i)
                {
                    LuaAPI.lua_pushnumber(L, i + 1);
                    LuaAPI.xlua_pgettable(L, 1);

                    var item = Convert.ChangeType(translator.GetObject(L, -1), otype);

                    arr.SetValue(item, i);
                    LuaAPI.lua_pop(L, 1);
                }

                LuaAPI.lua_settop(L, top);

                translator.Push(L, arr);
                return 1;
            }
            else if (LuaAPI.lua_isnumber(L, 1))
            {
                int len = (int)LuaAPI.lua_tonumber(L, 1);
                if (len < 0)
                {
                    len = 0;
                }
                Type otype = (Type)translator.FastGetCSObj(L, 2);
                if (otype == null)
                {
                    otype = typeof(object);
                }
                Array arr = Array.CreateInstance(otype, len);

                translator.Push(L, arr);
                return 1;
            }
            else if (LuaAPI.lua_isstring(L, 1))
            {
                var bytes = LuaAPI.lua_tobytes(L, 1);
                translator.Push(L, bytes);
                return 1;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncDict(IntPtr L)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

            if (LuaAPI.lua_istable(L, 1))
            {
                var top = LuaAPI.lua_gettop(L);

                IDictionary dict = null;

                Type ktype = (Type)translator.FastGetCSObj(L, 2);
                Type vtype = (Type)translator.FastGetCSObj(L, 3);
                if (ktype == null)
                {
                    ktype = typeof(object);
                }
                if (vtype == null)
                {
                    vtype = typeof(object);
                }
                dict = typeof(Dictionary<,>).MakeGenericType(ktype, vtype).GetConstructor(new Type[0]).Invoke(null) as IDictionary;
                LuaAPI.lua_pushnil(L);

                while (LuaAPI.lua_next(L, -2) != 0)
                {
                    object key = translator.GetObject(L, -2);
                    object val = translator.GetObject(L, -1);

                    dict.Add(Convert.ChangeType(key, ktype), Convert.ChangeType(val, vtype));

                    LuaAPI.lua_pop(L, 1);
                }
                //l.pop(1);

                LuaAPI.lua_settop(L, top);
                translator.Push(L, dict);
                return 1;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncTable(IntPtr L)
        {
            if (LuaAPI.lua_istable(L, 1))
            {
                LuaAPI.lua_pushvalue(L, 1);
                return 1;
            }

            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

            var obj = translator.SafeGetCSObj(L, 1);
            var lobj = obj as IList;
            if (lobj != null)
            {
                LuaAPI.lua_newtable(L);
                for (int i = 0; i < lobj.Count; ++i)
                {
                    LuaAPI.lua_pushnumber(L, i + 1);
                    translator.Push(L, lobj[i]);
                    LuaAPI.xlua_psettable(L, -3);
                }
                return 1;
            }
            var dobj = obj as IDictionary;
            if (dobj != null)
            {
                LuaAPI.lua_newtable(L);
                foreach (DictionaryEntry kvp in dobj)
                {
                    translator.Push(L, kvp.Key);
                    translator.Push(L, kvp.Value);
                    LuaAPI.xlua_psettable(L, -3);
                }
                return 1;
            }
            return 0;
        }

        public static int CSFuncGetLangValue(IntPtr L, bool isStringType)
        {
            var oldtop = LuaAPI.lua_gettop(L);
            if (LuaAPI.lua_istable(L, 1))
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

                LuaAPI.lua_pushnumber(L, 1);
                LuaAPI.xlua_pgettable(L, 1);

                string key = LuaAPI.lua_tostring(L, -1);
                LuaAPI.lua_pop(L, 1);
                var len = LuaAPI.xlua_objlen(L, 1);

                string[] args = new string[len - 1];
                for (int i = 2; i <= len; i++)
                {
                    LuaAPI.lua_pushnumber(L, i);
                    LuaAPI.xlua_pgettable(L, 1);
                    args[i - 2] = LuaAPI.lua_tostring(L, -1);
                    LuaAPI.lua_pop(L, 1);
                }
                string val = LanguageConverter.GetLangValue(key, args);

                if (isStringType)
                {
                    LuaAPI.lua_pushstring(L, val);
                }
                else
                {
                    translator.Push(L, val);
                }
            }
            return LuaAPI.lua_gettop(L) - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncGetLangValueOfUserDataType(IntPtr l)
        {
            return CSFuncGetLangValue(l, false);
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncGetLangValueOfStringType(IntPtr l)
        {
            return CSFuncGetLangValue(l, true);
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncConvert(IntPtr L)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            var obj = translator.GetObject(L, 1);
            var otype = translator.FastGetCSObj(L, 2) as Type;
            if (otype != null)
            {
                var ret = Convert.ChangeType(obj, otype);
                translator.Push(L, ret);
                return 1;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncIs(IntPtr L)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            var obj = translator.GetObject(L, 1);
            var otype = (Type)translator.FastGetCSObj(L, 2);
            bool rv = otype != null && otype.IsInstanceOfType(obj);
            LuaAPI.lua_pushboolean(L, rv);
            return 1;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncIsNull(IntPtr L)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            var type = LuaAPI.lua_type(L, 1);
            if (type == LuaTypes.LUA_TUSERDATA)
            {
                var obj = translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, obj == null || obj.Equals(null));
            }
            else
            {
                LuaAPI.lua_pushboolean(L, type <= 0);
            }
            return 1;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncEncryptPostData(IntPtr L)
        {
            //ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

            //var oldtop = LuaAPI.lua_gettop(L);

            //if (oldtop >= 1 && LuaAPI.lua_isstring(L, 1))
            //{
            //    var data = System.Text.Encoding.UTF8.GetBytes(LuaAPI.lua_tostring(L, 1));
            //    string token = null;
            //    ulong seq = 0;

            //    if (oldtop >= 2 && LuaAPI.lua_type(L, 2) > 0)
            //    {
            //        token = LuaAPI.lua_tostring(L, 2);
            //    }
            //    if (oldtop >= 3 && LuaAPI.lua_type(L, 3) > 0)
            //    {
            //        seq = (ulong)LuaAPI.lua_tonumber(L, 3);
            //    }
            //    var encrypted = PlatDependant.EncryptPostData(data, token, seq);
            //    if (encrypted != null)
            //    {
            //        translator.PushByType(L, encrypted);
            //    }
            //}
            //return LuaAPI.lua_gettop(L) - oldtop;
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncParseResponse(IntPtr L)
        {
            //ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

            //var oldtop = LuaAPI.lua_gettop(L);
            //var httpreq = translator.FastGetCSObj(L, 1) as Capstones.UnityFramework.HttpRequest;

            //string token = null;
            //ulong seq = 0;
            //if (oldtop >= 2 && LuaAPI.lua_type(L, 2) > 0)
            //{
            //    token = LuaAPI.lua_tostring(L, 2);
            //}
            //if (oldtop >= 3 && LuaAPI.lua_type(L, 3) > 0)
            //{
            //    seq = (ulong)LuaAPI.lua_tonumber(L, 3);
            //}

            //string resp = "";
            //if (httpreq != null)
            //{
            //    resp = httpreq.ParseResponse(token, seq);
            //}
            //LuaAPI.lua_pushstring(L, resp);

            //return 1;
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncGetCapID(IntPtr L)
        {
            var capID = GetCapID();
            LuaAPI.lua_pushstring(L, capID);
            return 1;
        }

        private static string _cached_Capid;

        public static string GetCapID()
        {
            if (_cached_Capid == null)
            {
                _cached_Capid = Capstones.UnityEngineEx.IsolatedPrefs.IsolatedID;
            }
            return _cached_Capid;
        }
        public static string ReloadCapID()
        {
            _cached_Capid = null;
            return GetCapID();
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncToLuaString(IntPtr L)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            var obj = translator.GetObject(L, 1);
            LuaAPI.lua_pushstring(L, obj as string);
            return 1;
        }

        [AOT.MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int CSFuncCheckZipValid(IntPtr L)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            string zipPath = translator.GetObject(L, 1) as string;

            bool isValid = false;
            var stream = PlatDependant.OpenRead(zipPath);
            if (stream != null)
            {
                var zip = new Unity.IO.Compression.ZipArchive(stream, Unity.IO.Compression.ZipArchiveMode.Read);
                isValid = (zip != null) && (zip.Entries != null);
                if (isValid)
                {
                    var etor = zip.Entries.GetEnumerator();
                    while (etor.MoveNext())
                    {
                        GLog.LogInfo("zip entry name : " + etor.Current.FullName);
                    }
                }

                if (zip != null)
                {
                    zip.Dispose();
                }
                stream.Dispose();
            }

            LuaAPI.lua_pushboolean(L, isValid);

            return 1;
        }

        #endregion
    }
}
