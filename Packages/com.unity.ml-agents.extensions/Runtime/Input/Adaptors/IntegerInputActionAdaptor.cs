#if MLA_INPUT_SYSTEM && UNITY_2019_4_OR_NEWER
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Unity.MLAgents.Extensions.Input
{
    /// <summary>
    /// Translates data from a <see cref="UnityEngine.InputSystem.Controls.IntegerControl"/>.
    /// </summary>
    public class IntegerInputActionAdaptor : IRLActionInputAdaptor
    {
        // TODO need to figure out how we can infer the branch size from here.
        /// <inheritdoc cref="IRLActionInputAdaptor.GetActionSpecForInputAction"/>
        public ActionSpec GetActionSpecForInputAction(InputAction action)
        {
            return ActionSpec.MakeDiscrete(2);
        }

        /// <inheritdoc cref="IRLActionInputAdaptor.QueueInputEventForAction"/>
        public void QueueInputEventForAction(InputAction action, InputControl control, ActionSpec actionSpec, in ActionBuffers actionBuffers)
        {
            var val = actionBuffers.DiscreteActions[0];
            InputSystem.QueueDeltaStateEvent(control, val);
        }

        /// <inheritdoc cref="IRLActionInputAdaptor.WriteToHeuristic"/>
        public void WriteToHeuristic(InputAction action, in ActionBuffers actionBuffers)
        {
            var actions = actionBuffers.DiscreteActions;
            var val = action.ReadValue<int>();
            actions[0] = val;
        }
    }
}
#endif // MLA_INPUT_SYSTEM && UNITY_2019_4_OR_NEWER
