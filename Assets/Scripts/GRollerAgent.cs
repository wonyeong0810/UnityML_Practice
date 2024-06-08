using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
//mlAgent 사용시 포함해야 됨

public class gRollerAgent : Agent
{
    Rigidbody rBody;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;
    public override void OnEpisodeBegin()
    {
        //새로운 애피소드 시작시, 다시 에이전트의 포지션의 초기화
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0) //에이전트가 floor 아래로 떨어진 경우 추가 초기화
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        //타겟의 위치는 에피소드 시작시 랜덤하게 변경된다.
        // Move the target to a new spot
        Target.localPosition = new Vector3(Random.value * 8 - 4,
                                           0.5f,
                                           Random.value * 8 - 4);
    }

    /// <summary>
    /// 강화학습 프로그램에게 관측정보를 전달
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        //타겟과 에이전트의 포지션을 전달한다.
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        //현재 에이전트의 이동량을 전달한다.
        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    /// <summary>
    /// 강화학습을 위한, 강화학습을 통한 행동이 결정되는 곳
    /// </summary>
    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //학습을 위한, 학습된 정보를 해석하여 이동을 시킨다.

        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        //타겟을 찻을시 리워드점수를 주고, 에피소드를 종료시킨다.
        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        //판 아래로 떨어지면 학습이 종료된다.
        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    /// <summary>
    /// 해당 함수는 직접조작 혹은 규칙성있는 코딩으로 조작시키기 위한 함수
    /// </summary>
    /// <param name="actionsOut"></param>

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
