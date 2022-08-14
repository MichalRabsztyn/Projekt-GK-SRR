using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjLayer;
    [SerializeField] LayerMask solidForegroundLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask portalLayer;

    public static GameLayers i{get;set;}
    private void Awake()
    {
        i = this;
    }


    public LayerMask SolidLayer
    {
        get => solidObjLayer;
    }
    
    public LayerMask SolidBackgroundLayer
    {
        get => solidObjLayer;
    }

    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }

    public LayerMask GrassLayer
    {
        get => grassLayer;
    }
    
    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }

    public LayerMask PortalLayer
    {
        get => portalLayer;
    }

    public LayerMask TriggerableLayers
    {
        get => grassLayer | portalLayer;
    }
}
