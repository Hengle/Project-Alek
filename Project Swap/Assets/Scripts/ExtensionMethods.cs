using System.Collections;
using Characters.PartyMembers;
using UnityEngine;

public static class ExtensionMethods
{
    public static IEnumerator SwapPosition(this Transform transform, Transform target, float swapSpeed)
    {
        Logger.Log("Should be swapping right about now...");
        var position = transform.position;
        var offsetPosition1 = new Vector3(position.x, position.y, position.z + 4);
        
        var position1 = target.position;
        var offsetPosition2 = new Vector3(position1.x, position1.y, position1.z - 4);

        var targetPosition1 = offsetPosition1;
        var targetPosition2 = offsetPosition2;

        while (transform.position != targetPosition2 
               && target.position != targetPosition1)
        {
            var characterSwapping = transform;
            var currentSwapTarg = target;
                
            if (Mathf.Abs((characterSwapping.position - currentSwapTarg.position).x) <= 0.2f) {
                targetPosition1 = position;
                targetPosition2 = position1;
            }
                
            characterSwapping.position = Vector3.MoveTowards
                (characterSwapping.position, targetPosition2, swapSpeed * Time.fixedDeltaTime);
                
            currentSwapTarg.position = Vector3.MoveTowards
                (currentSwapTarg.position, targetPosition1, swapSpeed * Time.fixedDeltaTime);
            
            yield return new WaitForEndOfFrame();
        }
        
        var thisIndex = transform.GetSiblingIndex();
        var targetIndex = target.GetSiblingIndex();

        transform.SetSiblingIndex(targetIndex);
        target.SetSiblingIndex(thisIndex);
        
        MenuController.updateSelectables.Invoke();
    }

    public static void SwapSiblingIndex(this Transform transform, Transform target)
    {
        var unitIndex = transform.GetSiblingIndex();
        var targetIndex = target.GetSiblingIndex();
                
        transform.SetSiblingIndex(targetIndex);
        target.SetSiblingIndex(unitIndex);
    }
} 