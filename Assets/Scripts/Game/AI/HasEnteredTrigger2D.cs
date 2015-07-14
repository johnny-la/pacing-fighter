using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[BehaviorDesigner.Runtime.Tasks.TaskDescription("Returns success when an object enters the 2D trigger.")]
public class HasEnteredTrigger2D : BehaviorDesigner.Runtime.Tasks.Conditional
{
	public SharedInt layer = 1;

    private bool enteredTrigger = false;

    public override TaskStatus OnUpdate()
    {
        return enteredTrigger ? TaskStatus.Success : TaskStatus.Failure;
    }

    public override void OnEnd()
    {
        enteredTrigger = false;
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (layer.Value.Equals(other.gameObject.layer)) {
            enteredTrigger = true;
        }
    }

    public override void OnReset()
    {
		layer = 1;
    }
}
