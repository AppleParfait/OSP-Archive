#if MLA_INPUT_SYSTEM && UNITY_2019_4_OR_NEWER
using System;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Unity.MLAgents.Extensions.Input
{
    /// <summary>
    /// Translates data from any control that extends from <see cref="InputControl{Vector2}"/>.
    /// </summary>
    public class Vector2InputActionAdaptor : IRLActionInputAdaptor
    {
        /// <inheritdoc cref="IRLActionInputAdaptor.GetActionSpecForInputAction"/>
        public ActionSpec GetActionSpecForInputAction(InputAction action)
        {
            // TODO create the action spec based on what controls back the action
            return ActionSpec.MakeContinuous(2);
        }

        /// <inheritdoc cref="IRLActionInputAdaptor.QueueInputEventForAction"/>
        public void QueueInputEventForAction(InputAction action,
            InputControl control,
            ActionSpec actionSpec,
            in ActionBuffers actionBuffers)
        {
            var x = actionBuffers.ContinuousActions[0];
            var y = actionBuffers.ContinuousActions[1];
            InputSystem.QueueDeltaStateEvent(control, new Vector2(x, y));
        }

        /// <inheritdoc cref="IRLActionInputAdaptor.WriteToHeuristic"/>
        public void WriteToHeuristic(InputAction action, in ActionBuffers actionBuffers)
        {
            var value = action.ReadValue<Vector2>();
            var continuousActions = actionBuffers.ContinuousActions;
            continuousActions[0] = value.x;
            continuousActions[1] = value.y;
        }

    }
}
#endif // MLA_INPUT_SYSTEM && UNITY_2019_4_OR_NEWER
