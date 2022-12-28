// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class MenuButtonBehavior : MonoBehaviour
// {
//     public MenuButtonBehavior[] innerMenuButtons;
//     public Vector3 Location { get; private set; }
//     public float Size { get; private set; }
//     [SerializeField] private int numOfInnerButtonsInARow;
//     [SerializeField] private bool isRunButton;
//     [SerializeField] private bool isFightButton;
//     private void Start()
//     {
//         Location = gameObject.transform.position;
//         Size = GetComponent<SpriteRenderer>().bounds.size.x;
//     }
//
//     public void Action()
//     {
//         if (isRunButton)
//         {
//             //Roll a chance to leave battle by the enemies and player's speed.
//             // FightManager.Instance.ActionHasTaken = true;
//             return;
//         }
//
//         if (isFightButton)
//         {
//             
//         }
//         // PointerBehavior.Instance.SetToNewMenu(innerMenuButtons,numOfInnerButtonsInARow);
//         //instatiate a new window at the canvas.
//     }
// }
