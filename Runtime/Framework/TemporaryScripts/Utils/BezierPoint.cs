using UnityEngine;

namespace BezierSolution
{
	public class BezierPoint
	{
		public enum HandleMode { Free, Aligned, Mirrored };

		[SerializeField]
		[HideInInspector]
		private Vector3 m_position;
		public Vector3 position
		{
			get
			{
				return m_position;
			}
			set { m_position = value; }
		}

		[SerializeField]
		[HideInInspector]
		private Vector3 m_precedingControlPointPosition;
		public Vector3 precedingControlPointPosition
		{
			get
			{
				return m_precedingControlPointPosition;
			}
			set
			{
				m_precedingControlPointPosition = value;

				if( m_handleMode == HandleMode.Aligned )
				{
					m_followingControlPointPosition = m_position - ( m_precedingControlPointPosition - m_position ).normalized *
																   ( m_followingControlPointPosition - m_position ).magnitude;
				}
				else if( m_handleMode == HandleMode.Mirrored )
				{
					m_followingControlPointPosition = 2f * m_position - m_precedingControlPointPosition;
				}
			}
		}

		[SerializeField]
		[HideInInspector]
		private Vector3 m_followingControlPointPosition;
		public Vector3 followingControlPointPosition
		{
			get
			{
				return m_followingControlPointPosition;
			}
			set
			{
				m_followingControlPointPosition = value;

				if( m_handleMode == HandleMode.Aligned )
				{
					m_precedingControlPointPosition = m_position - ( m_followingControlPointPosition - m_position ).normalized *
																	( m_precedingControlPointPosition - m_position ).magnitude;
				}
				else if( m_handleMode == HandleMode.Mirrored )
				{
					m_precedingControlPointPosition = 2f * m_position - m_followingControlPointPosition;
				}
			}
		}

		[SerializeField]
		[HideInInspector]
		private HandleMode m_handleMode = HandleMode.Mirrored;
		public HandleMode handleMode
		{
			get
			{
				return m_handleMode;
			}
			set
			{
				m_handleMode = value;
			}
		}
	}
}