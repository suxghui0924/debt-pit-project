using UnityEngine;

public sealed class WorldInteractionTrigger : MonoBehaviour
{
    public bool PlayerInside { get; private set; }

    public static WorldInteractionTrigger Create(Transform owner, string triggerName)
    {
        Transform existing = owner.Find(triggerName);
        if (existing != null)
        {
            Collider existingCollider = existing.GetComponent<Collider>();
            if (existingCollider == null) existingCollider = existing.gameObject.AddComponent<BoxCollider>();
            existingCollider.isTrigger = true;
            WorldInteractionTrigger existingTrigger = existing.GetComponent<WorldInteractionTrigger>();
            return existingTrigger != null ? existingTrigger : existing.gameObject.AddComponent<WorldInteractionTrigger>();
        }

        // Map의 Computer/Shop/Object/Task에는 이미 Is Trigger 콜라이더가 있다.
        // 그 콜라이더를 그대로 사용해야 모델 스케일과 실제 감지 범위가 어긋나지 않는다.
        Collider ownerCollider = owner.GetComponent<Collider>();
        if (ownerCollider != null && ownerCollider.isTrigger)
        {
            WorldInteractionTrigger ownerTrigger = owner.GetComponent<WorldInteractionTrigger>();
            return ownerTrigger != null ? ownerTrigger : owner.gameObject.AddComponent<WorldInteractionTrigger>();
        }

        GameObject trigger = new(triggerName, typeof(BoxCollider), typeof(WorldInteractionTrigger));
        trigger.transform.SetParent(owner, false);
        trigger.transform.localPosition = new Vector3(0f, .35f, 0f);
        BoxCollider box = trigger.GetComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(1.35f, 1.25f, 1.35f);
        return trigger.GetComponent<WorldInteractionTrigger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementController>() != null) PlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementController>() != null) PlayerInside = false;
    }
}
