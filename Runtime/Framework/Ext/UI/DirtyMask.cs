using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirtyMask : Mask
{
    protected override void OnEnable()
    {
        base.OnEnable();

        if (graphic != null)
        {
            graphic.canvasRenderer.hasPopInstruction = false;
            graphic.canvasRenderer.popMaterialCount = 0;
        }
    }

    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        var rv = base.GetModifiedMaterial(baseMaterial);
        if (graphic != null)
        {
            graphic.canvasRenderer.hasPopInstruction = false;
            graphic.canvasRenderer.popMaterialCount = 0;
        }
        return rv;
    }
}
