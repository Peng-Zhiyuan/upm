using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputUtil
{

    public static Touch? TryGetTouchOrSimulateTouch()
    {
        var simulateTouch = GetMouseSimulateTouch();
        if (simulateTouch != null)
        {
            return simulateTouch;
        }
        var touchCount = Input.touchCount;
        if (touchCount > 0)
        {
            var firstTouch = Input.touches[0];
            return firstTouch;
        }
        return null;
    }

    //static bool isLeftMousePressed;
    private static Touch? GetMouseSimulateTouch()
    {


        var isLeftMouseButtonDown = Input.GetMouseButtonDown(0);
        if (isLeftMouseButtonDown)
        {
            var touch = new Touch();
            var mousePosition = Input.mousePosition;
            touch.position = mousePosition;
            touch.phase = TouchPhase.Began;
            //isLeftMousePressed = true;
            return touch;
        }

        var isLeftMouseButtonUp = Input.GetMouseButtonUp(0);
        if (isLeftMouseButtonUp)
        {
            var touch = new Touch();
            var mousePosition = Input.mousePosition;
            touch.position = mousePosition;
            touch.phase = TouchPhase.Ended;
            //isLeftMousePressed = false;
            return touch;
        }

        var isLeftMouseButtonPressed = Input.GetMouseButton(0);
        if (isLeftMouseButtonPressed)
        {
            var touch = new Touch();
            var mousePosition = Input.mousePosition;
            touch.position = mousePosition;
            touch.phase = TouchPhase.Moved;
            //isLeftMousePressed = true;
            return touch;
        }

        return null;

    }
}
