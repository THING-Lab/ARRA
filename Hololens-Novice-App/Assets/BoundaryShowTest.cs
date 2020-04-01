using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;

namespace Microsoft.MixedReality.Toolkit
{
    public class BoundaryShowTest : MonoBehaviour
    {

        void HideBoundary() {
            if (CoreServices.BoundarySystem != null)
            {
                var boundarySystem = CoreServices.BoundarySystem;
                boundarySystem.ShowFloor = false;
                boundarySystem.ShowPlayArea = false;
                boundarySystem.ShowTrackedArea = false;
                boundarySystem.ShowBoundaryWalls = false;
                boundarySystem.ShowBoundaryCeiling = false;
            }
        }

        void ShowBoundary() {
            if (CoreServices.BoundarySystem != null)
            {
                var boundarySystem = CoreServices.BoundarySystem;
                boundarySystem.ShowFloor = true;
                boundarySystem.ShowPlayArea = true;
                boundarySystem.ShowTrackedArea = true;
                boundarySystem.ShowBoundaryWalls = true;
                boundarySystem.ShowBoundaryCeiling = true;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            // CoreServices.BoundarySystem.ShowBoundaryWalls = false;
            HideBoundary();
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}