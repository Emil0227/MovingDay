using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private InputManager m_InputManager;
    [SerializeField]
    private Grid m_Grid;
    [SerializeField]
    private ObjectsDataBaseSO m_DataBase;
    [SerializeField]
    private int m_ObjectCount;

    private int m_SelectedObjectIndex = -1;
    private bool m_IsItemSelected = false;
    private GameObject m_SelectedObject;
    private GridData m_FloorData, m_FurnitureData;
    private Vector3Int m_CurrentGridPosition;


    private void Start()
    {
        StopPlacement();
        //m_FloorData = new();
        //m_FurnitureData = new();
    }

    private void Update()
    {
        if (m_SelectedObjectIndex < 0)
            return;

        //bool placementValidity = CheckPlacementValidity(m_CurrentGridPosition, m_SelectedObjectIndex);
        //if (!placementValidity)
        //    return;

        ItemFollowMouse();
    }

    private void ItemFollowMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        
        if (m_IsItemSelected)
        {
            //检测射线是否击中物品对应的layer
            if (Physics.Raycast(ray, out hit, 1000, m_DataBase.ObjectDataList[m_SelectedObjectIndex].PlacementLayerMask))//100是射线长度
            {
                //物品按照格子跳动
                m_CurrentGridPosition = m_Grid.WorldToCell(hit.point);
                m_SelectedObject.transform.position = m_Grid.CellToWorld(m_CurrentGridPosition);
                //设置物品为不透明
                m_SelectedObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            }
            else
            {
                //物品跟随鼠标移动
                Vector3 screenPos = Camera.main.WorldToScreenPoint(m_SelectedObject.transform.position);//获取需要移动物品的世界转屏幕坐标
                mousePos = Input.mousePosition;//获取鼠标位置
                mousePos.z = screenPos.z;//因为鼠标只有X，Y轴，所以要赋予给鼠标Z轴
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);//把鼠标的屏幕坐标转换成世界坐标
                m_SelectedObject.transform.position = worldPos;//控制物品移动

                //设置物品为透明
                m_SelectedObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
            }
        }
    }

    //public void StartPlacement(int ID)
    //{
    //    StopPlacement();
    //    m_SelectedObjectIndex = m_DataBase.ObjectDataList.FindIndex(obj => obj.ID == ID);
    //    if (m_SelectedObjectIndex < 0)
    //    {
    //        Debug.LogError($"No ID Found {ID}");
    //        return;
    //    }
    //    m_GridVisualization.SetActive(true);
    //    m_CellIndicator.SetActive(true);
    //    m_InputManager.OnClicked += PlaceStructure;
    //    m_InputManager.OnExit += StopPlacement;
    //}

    public void StartPlacement()//从盒子里依次取出物品
    {
        StopPlacement();
        if (m_SelectedObjectIndex > m_ObjectCount)
        {
            return; 
        }
        m_SelectedObjectIndex += 1;
        m_SelectedObject = Instantiate(m_DataBase.ObjectDataList[m_SelectedObjectIndex].Prefab);//生成物品
        print(m_SelectedObject);
        m_IsItemSelected = true;
        m_InputManager.OnClicked += PlaceStructure;
        m_InputManager.OnExit += StopPlacement;
    }

    private void StopPlacement()
    {
        m_InputManager.OnClicked -= PlaceStructure;
        m_InputManager.OnExit -= StopPlacement;
    }

    //将物品放进格子
    private void PlaceStructure()
    {
        if (m_InputManager.IsPointerOverUI())
        {
            return;
        }
        ItemFollowMouse();
        //bool placementValidity = CheckPlacementValidity(m_CurrentGridPosition, m_SelectedObjectIndex);
        //if (!placementValidity)
        //    return;
        m_IsItemSelected = false;
    }

    //private bool CheckPlacementValidity(Vector3Int m_CurrentGridPosition, int m_SelectedObjectIndex)
    //{
    //    throw new NotImplementedException();
    //}
}
