using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject m_MouseIndicator;
    [SerializeField]
    private InputManager m_InputManager;
    [SerializeField]
    private Grid m_Grid;

    [SerializeField]
    private ObjectsDataBaseSO m_DataBase;
    private int m_SelectedObjectIndex = -1;

    private bool m_IsItemSelected = false;
    private GameObject m_SelectedObject;

    private void Start()
    {
        StopPlacement();
    }

    private void Update()
    {
        if (m_SelectedObjectIndex < 0)
            return;

        //���λ��
        Vector3 mousePosition = m_InputManager.GetSelectedMapPosition();
        m_MouseIndicator.transform.position = mousePosition;
        
        //��Ʒ��������ƶ�
        if (m_IsItemSelected)
        {
            Vector3Int gridPosition = m_Grid.WorldToCell(mousePosition);// ��λ������ڸ��ӵ�����
            m_SelectedObject.transform.position = m_Grid.CellToWorld(gridPosition);
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

    public void StartPlacement()//����ȡ����Ʒ
    {
        StopPlacement();
        if (m_SelectedObjectIndex > 3)
        {
            return; 
        }
        m_SelectedObjectIndex += 1;
        m_SelectedObject = Instantiate(m_DataBase.ObjectDataList[m_SelectedObjectIndex].Prefab);//�����±�������Ʒ
        m_IsItemSelected = true;
        m_InputManager.OnClicked += PlaceStructure;
        m_InputManager.OnExit += StopPlacement;
    }

    private void StopPlacement()
    {
        m_InputManager.OnClicked -= PlaceStructure;
        m_InputManager.OnExit -= StopPlacement;
    }

    private void PlaceStructure()//����Ʒ����ָ���ĸ���
    {
        if (m_InputManager.IsPointerOverUI())
        {
            return;
        }
        Vector3 mousePosition = m_InputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = m_Grid.WorldToCell(mousePosition);// ��λ������ڸ��ӵ�����
        m_SelectedObject.transform.position = m_Grid.CellToWorld(gridPosition);
        m_IsItemSelected = false;
    }
}
