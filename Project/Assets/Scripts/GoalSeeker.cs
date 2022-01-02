using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(AgentMovement))]
public class GoalSeeker : Agent
{
    [SerializeField] private Transform _goal;
    [SerializeField] private PlatformBehavior _platformBehavior;
    private AgentMovement _agentMovement;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(_goal.localPosition);
        sensor.AddObservation(_agentMovement.IsGrounded() ? 1 : 0);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[0] = Input.GetAxisRaw("Horizontal");
        continousActions[1] = Input.GetAxisRaw("Vertical");
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    public override void Initialize()
    {
        _agentMovement = GetComponent<AgentMovement>();
        _agentMovement.onJumpDelegate += OnJump;
    }

    private void OnJump()
    {
        SetReward(-0.01f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotation = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float forwardMovement = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        bool jump = actions.DiscreteActions[0] == 1 ? true : false;

        _agentMovement.RotateAgent(rotation);
        _agentMovement.MoveAgent(forwardMovement);
        if (jump)
        {
            _agentMovement.Jump();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            SetReward(1f);
            _platformBehavior.SetStageSuccess(true);
            EndEpisode();
        }
    }

    private void Update()
    {
        if (transform.localPosition.y < -1f)
        {
            SetReward(-1f);
            _platformBehavior.SetStageSuccess(false);
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        _platformBehavior.CreateStage();
        transform.localPosition = new Vector3(0f, 1f, 0f);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        base.WriteDiscreteActionMask(actionMask);
    }
}
