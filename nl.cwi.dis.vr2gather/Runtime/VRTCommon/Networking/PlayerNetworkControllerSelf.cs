using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRT.Orchestrator.Wrapping;

namespace VRT.Pilots.Common
{
    public class PlayerNetworkControllerSelf : PlayerNetworkControllerBase
    {
		private float _LastSendTime;
		[Tooltip("Where to get head orientation from")]
		public Transform camTransform;

		
		public override void SetupPlayerNetworkController(PlayerControllerBase _playerController, bool local, string _userId)
		{
			if (!local)
            {
				Debug.LogError($"{Name()}: SetupPlayerNetworkControllerPlayer with local=false");
            }
			_IsLocalPlayer = true;
			UserId = _userId;
			playerController = _playerController;
		}

		void Update()
		{
			if (_LastSendTime + (1.0f / SendRate) <= Time.realtimeSinceStartup)
			{
				SendPlayerData();
			}
		}
		void SendPlayerData()
		{
			if (playerController == null)
			{
				Debug.LogError($"{Name()}: SendPlayerData with no playerController. Probably SetupPlayerNetworkController was not called.");
				return;
			}
			float BodySize = 0;
			GameObject currentRepresentation = playerController.GetRepresentationGameObject();
            if (currentRepresentation != null && currentRepresentation.activeInHierarchy)
            {
                BodySize = currentRepresentation.transform.localScale.y;
            }
            if (AlternativeUserRepresentation != null && AlternativeUserRepresentation.activeInHierarchy)
            {
                BodySize = AlternativeUserRepresentation.transform.localScale.y;
            }
            // For debugging mainly: also copy head position/orientation for the self user
            if (Head3TransformAlsoMove != null)
			{
				Head3TransformAlsoMove.rotation = camTransform.rotation;
				Head3TransformAlsoMove.position = camTransform.position;
			}
			var data = new NetworkPlayerData
			{
				BodyPosition = BodyTransform.position,
				BodyOrientation = BodyTransform.rotation,
				HeadPosition = camTransform.position,
				HeadOrientation = camTransform.rotation,
				LeftHandPosition = LeftHandTransform.position,
				LeftHandOrientation = LeftHandTransform.rotation,
				RightHandPosition = RightHandTransform.position,
				RightHandOrientation = RightHandTransform.rotation,
				representation = playerController.userRepresentation,
				BodySize = BodySize
			};

			if (OrchestratorController.Instance.UserIsMaster)
			{
				OrchestratorController.Instance.SendTypeEventToAll(data);
			}
			else
			{
				OrchestratorController.Instance.SendTypeEventToMaster(data);
			}
			// Print a warning if it was preposterously long ago that we last sent this message
			if (_LastSendTime > 0 && Time.realtimeSinceStartup > _LastSendTime + 10.0)
            {
				Debug.LogWarning($"{Name()}: No SendPlayerData() calls in {Time.realtimeSinceStartup - _LastSendTime} seconds");
            }
			_LastSendTime = Time.realtimeSinceStartup;
		}
	}
}
