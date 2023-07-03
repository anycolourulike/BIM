using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapView : MonoBehaviour
{
   [SerializeField] Camera mapCam;

   public void ShowMap()
   {
       mapCam.depth = 0;
   }

   public void HideMap()
   {
       mapCam.depth = -2;
   }
}
