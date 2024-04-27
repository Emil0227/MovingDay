using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public event Action OnClicked, OnExit;

    [SerializeField] 
    private Camera m_Camera;
    [SerializeField] 
    private LayerMask m_PlacementLayerMask;
    
    private Vector3 m_LastPosition;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnClicked?.Invoke();
        if (Input.GetMouseButtonDown(1))
            OnExit?.Invoke();
    }

    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = m_Camera.nearClipPlane;
        Ray ray = m_Camera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, m_PlacementLayerMask))//100是射线的长度
        {
            m_LastPosition = hit.point;
        }
        return m_LastPosition;
    }
}
