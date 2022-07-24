#if MLA_INPUT_TESTS && UNITY_2019_4_OR_NEWER
using NUnit.Framework;
using Unity.Barracuda;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Extensions.Input;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.MLAgents.Extensions.Tests.Runtime.Input
{
    class TestAdaptor : IRLActionInputAdaptor
    {
        public bool eventQueued;
        public bool writtenToHeuristic;

        public ActionSpec GetActionSpecForInputAction(InputAction action)
        {
            return ActionSpec.MakeContinuous(1);
        }

        public void QueueInputEventForAction(InputAction action, InputControl control, ActionSpec actionSpec, in ActionBuffers actionBuffers)
        {
            eventQueued = true;
        }

        public void WriteToHeuristic(InputAction action, in ActionBuffers actionBuffers)
        {
            writtenToHeuristic = true;
        }

        public void Reset()
        {
            eventQueued = false;
            writtenToHeuristic = false;
        }
    }

    [TestFixture]
    public class InputActionActuatorTests
    {
        BehaviorParameters m_BehaviorParameters;
        InputActionActuator m_Actuator;
        TestAdaptor m_Adaptor;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject();
            m_BehaviorParameters = go.AddComponent<BehaviorParameters>();
            var action = new InputAction("action");
            m_Adaptor = new TestAdaptor();
            m_Actuator = new InputActionActuator(null, m_BehaviorParameters, action, m_Adaptor);
        }

        [Test]
        public void TestOnActionReceived()
        {
            m_BehaviorParameters.BehaviorType = BehaviorType.HeuristicOnly;
            m_Actuator.OnActionReceived(new ActionBuffers());
            m_Actuator.Heuristic(new ActionBuffers());
            Assert.IsFalse(m_Adaptor.eventQueued);
            Assert.IsTrue(m_Adaptor.writtenToHeuristic);
            m_Adaptor.Reset();

            m_BehaviorParameters.BehaviorType = BehaviorType.Default;
            m_Actuator.OnActionReceived(new ActionBuffers());
            Assert.IsFalse(m_Adaptor.eventQueued);
            m_Adaptor.Reset();

            m_BehaviorParameters.Model = ScriptableObject.CreateInstance<NNModel>();
            m_Actuator.OnActionReceived(new ActionBuffers());
            Assert.IsTrue(m_Adaptor.eventQueued);
            m_Adaptor.Reset();

            Assert.AreEqual(m_Actuator.Name, "InputActionActuator-action");
            m_Actuator.ResetData();
            m_Actuator.WriteDiscreteActionMask(null);
        }
    }
}
#endif // MLA_INPUT_TESTS && UNITY_2019_4_OR_NEWER
