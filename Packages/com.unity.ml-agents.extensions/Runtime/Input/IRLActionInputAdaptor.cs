#if MLA_INPUT_SYSTEM && UNITY_2019_4_OR_NEWER
using System;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

namespace Unity.MLAgents.Extensions.Input
{
    /// <summary>
    /// Implement this interface in order to customize how information is translated <see cref="InputControl"/>s
    /// and <see cref="ActionBuffers"/>.
    /// </summary>
    public interface IRLActionInputAdaptor
    {
        /// <summary>
        /// Generate an <see cref="ActionSpec"/> for a given action which determines how data is translated between
        /// the <see cref="InputSystem"/> and ML-Agents.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to based the <see cref="ActionSpec"/> from.</param>
        /// <returns>An <see cref="ActionSpec"/> instance based off the information in the <see cref="InputAction"/>.</returns>
        ActionSpec GetActionSpecForInputAction(InputAction action);

        /// <summary>
        /// Translates data from the <see cref="ActionBuffers"/> object to the <see cref="InputSystem"/>.
        /// </summary>
        /// <param name="action">The action associated with this adaptor.</param>
        /// <param name="control">The control which will write the event to the <see cref="InputSystem"/>.</param>
        /// <param name="actionSpec">The <see cref="ActionSpec"/> associated with this action and adaptor pair.</param>
        /// <param name="actionBuffers">The <see cref="ActionBuffers"/> object to read from.</param>
        void QueueInputEventForAction(InputAction action, InputControl control, ActionSpec actionSpec, in ActionBuffers actionBuffers);

        /// <summary>
        /// Writes data from the <paramref name="action"/> to the <paramref name="actionBuffers"/>.
        /// </summary>
        /// <param name="action">The <paramref name="action"/> to read data from.</param>
        /// <param name="actionBuffers">The <paramref name="actionBuffers"/> object to write data to.</param>
        void WriteToHeuristic(InputAction action, in ActionBuffers actionBuffers);
    }
}
#endif // MLA_INPUT_SYSTEM && UNITY_2019_4_OR_NEWER
