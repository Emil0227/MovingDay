using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static Connector;

public class BuildingManager : MonoBehaviour
{
    [Header("Build Objects")]
    [SerializeField] private List<GameObject> m_FloorObjectList = new List<GameObject>();
    [SerializeField] private List<GameObject> m_WallObjectList = new List<GameObject>();

    [Header("Build Settings")]
    [SerializeField] private SelectedBuildType m_CurrentBuildType;
    [SerializeField] private LayerMask m_ConnectorLayer;

    [Header("Destroy Settingd")]
    [SerializeField] private bool m_IsDestroying = false;
    private Transform m_LastHitDestroyTransform;
    private List<Material> m_LastHitMaterialList = new List<Material>();

    [Header("Ghost Settings")]
    [SerializeField] private Material m_GhostMaterialValid;
    [SerializeField] private Material m_GhostMaterialInvalid;
    [SerializeField] private float m_ConnectorOverlapRadius = 1;
    [SerializeField] private float m_MaxGroundAngle = 45f;

    [Header("Internal State")]
    [SerializeField] private bool m_IsBuilding = false;
    [SerializeField] private int m_CurrentBuildingIndex = 0;
    private GameObject m_GhostBuildGameobject;
    private bool m_IsGhostInValidPosition = false;
    private Transform m_ModelParentTrans = null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            m_IsBuilding = !m_IsBuilding;

        if (Input.GetKeyDown(KeyCode.V))
            m_IsDestroying = !m_IsDestroying;

        if (m_IsBuilding && !m_IsDestroying)
        {
            GhostBuild();
            if (Input.GetMouseButtonDown(0))
                PlaceBuild();
        }
        else if (m_GhostBuildGameobject)
        {
            Destroy(m_GhostBuildGameobject);
            m_GhostBuildGameobject = null;
        }

        if (m_IsDestroying)
        {
            GhostDestroy();

            if (Input.GetMouseButtonDown(0))
                DestroyBuild();
        }
    }

    private void GhostBuild()
    {
        GameObject currentBuild = GetCurrentBuild();
        CreateGhostPrefab(currentBuild);

        MoveGhostPrefabToRaycast();
        CheckBuildValidity();
    }

    private void CreateGhostPrefab(GameObject currentBuild)
    {
        if (m_GhostBuildGameobject == null)
        {
            m_GhostBuildGameobject = Instantiate(currentBuild);
            
            m_ModelParentTrans = m_GhostBuildGameobject.transform.GetChild(0);

            GhostifyModel(m_ModelParentTrans, m_GhostMaterialValid);
            GhostifyModel(m_GhostBuildGameobject.transform);
        }
    }

    private void MoveGhostPrefabToRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            m_GhostBuildGameobject.transform.position = hit.point;
        }
    }

    private void CheckBuildValidity()
    {
        Collider[] colliders = Physics.OverlapSphere(m_GhostBuildGameobject.transform.position, m_ConnectorOverlapRadius, m_ConnectorLayer);
        if (colliders.Length > 0)
        {
            GhostConnectBuild (colliders);
        }
        else
        {
            GhostSeperateBuild();
        }
    }

    private void GhostConnectBuild(Collider[] colliders)
    {
        Connector bestConnector = null;

        foreach (Collider collider in colliders)
        {
            Connector connector = collider.GetComponent<Connector>();

            if (connector.CanConnectTo)
            {
                bestConnector = connector;
                break;
            }
        }

        if (bestConnector == null || (m_CurrentBuildType == SelectedBuildType.floor && bestConnector.IsConnectedToFloor) || (m_CurrentBuildType == SelectedBuildType.wall && bestConnector.IsConnectedToWall))
        {
            GhostifyModel(m_ModelParentTrans, m_GhostMaterialInvalid);
            m_IsGhostInValidPosition = false;
            return;
        }

        SnapGhostPrefabToConnector(bestConnector);
    }

    private void SnapGhostPrefabToConnector(Connector connector)
    {
        Transform ghostConnectorTrans = FindSnapConnector(connector.transform, m_GhostBuildGameobject.transform.GetChild(1));
        m_GhostBuildGameobject.transform.position = connector.transform.position - (ghostConnectorTrans.position - m_GhostBuildGameobject.transform.position);
    
        if (m_CurrentBuildType == SelectedBuildType.wall)
        {
            Quaternion newRotation = m_GhostBuildGameobject.transform.rotation;
            newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.rotation.eulerAngles.y, newRotation.eulerAngles.z);
            m_GhostBuildGameobject.transform.rotation = newRotation;
        }

        GhostifyModel(m_ModelParentTrans, m_GhostMaterialValid);
        m_IsGhostInValidPosition = true;
    }

    private void GhostSeperateBuild()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (m_CurrentBuildType == SelectedBuildType.wall)
            {
                GhostifyModel(m_ModelParentTrans, m_GhostMaterialInvalid);
                m_IsGhostInValidPosition = false;
                return;
            }

            if (hit.collider.transform.root.CompareTag("Buildables"))
            {
                GhostifyModel(m_ModelParentTrans, m_GhostMaterialInvalid);
                m_IsGhostInValidPosition = false;
                return;
            }

            if (Vector3.Angle(hit.normal, Vector3.up) < m_MaxGroundAngle)
            {
                GhostifyModel(m_ModelParentTrans, m_GhostMaterialInvalid);
                m_IsGhostInValidPosition = true;
            }
            else
            {
                GhostifyModel(m_ModelParentTrans, m_GhostMaterialInvalid);
                m_IsGhostInValidPosition = true;
            }
        }
    }

    private Transform FindSnapConnector(Transform snapConnectorTrans, Transform ghostConnectorParentTrans)
    {
        ConnectorPosition oppositeConnectorTag = GetOppositePosition(snapConnectorTrans.GetComponent<Connector>());

        foreach (Connector connector in ghostConnectorParentTrans.GetComponentsInChildren<Connector>())
        {
            if (connector.ConnectorPos == oppositeConnectorTag)
                return connector.transform;
        }

        return null;
    }

    private ConnectorPosition GetOppositePosition (Connector connector)
    {
        ConnectorPosition position = connector.ConnectorPos;

        if (m_CurrentBuildType == SelectedBuildType.wall && connector.ConnectorParentType == SelectedBuildType.floor)
            return ConnectorPosition.Bottom;

        //把地板建在墙上
        if (m_CurrentBuildType == SelectedBuildType.floor && connector.ConnectorParentType == SelectedBuildType.wall && connector.ConnectorPos == ConnectorPosition.Top)
        {
            if (connector.transform.root.rotation.y == 0)
                return GetConnectorClosestToPlayer(true);
            else
                return GetConnectorClosestToPlayer(false);
        }

        switch (position)
        {
            case ConnectorPosition.Left:
                return ConnectorPosition.Right;
            case ConnectorPosition.Right:
                return ConnectorPosition.Left;
            case ConnectorPosition.Top:
                return ConnectorPosition.Bottom;
            case ConnectorPosition.Bottom:
                return ConnectorPosition.Top;
            case ConnectorPosition.Back:
                return ConnectorPosition.Front;
            case ConnectorPosition.Front:
                return ConnectorPosition.Back;
            default:
                return ConnectorPosition.Bottom;
        }
    }

    private ConnectorPosition GetConnectorClosestToPlayer(bool topBottom)
    {
        Transform cameraTrans = Camera.main.transform;

        if (topBottom)
            return cameraTrans.position.z >= m_GhostBuildGameobject.transform.position.z ? ConnectorPosition.Bottom : ConnectorPosition.Top;
        else
            return cameraTrans.position.x >= m_GhostBuildGameobject.transform.position.x ? ConnectorPosition.Left : ConnectorPosition.Right;
    }

    private void GhostifyModel(Transform modelParentTrans, Material ghostMaterial = null)
    {
        if (ghostMaterial != null)
        {
            foreach (MeshRenderer meshRenderer in modelParentTrans.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = ghostMaterial;
            }
        }
        else
        {
            foreach (Collider modelColliders in modelParentTrans.GetComponentsInChildren<Collider>())
            {
                modelColliders.enabled = false;
            }
        }
    }

    private GameObject GetCurrentBuild()
    {
        switch(m_CurrentBuildType)
        {
            case SelectedBuildType.floor:
                return m_FloorObjectList[m_CurrentBuildingIndex];
            case SelectedBuildType.wall:
                return m_WallObjectList[m_CurrentBuildingIndex];
        }

        return null;
    }

    private void PlaceBuild()
    {
        if (m_GhostBuildGameobject != null && m_IsGhostInValidPosition)
        {
            GameObject newBuild = Instantiate (GetCurrentBuild(), m_GhostBuildGameobject.transform.position, m_GhostBuildGameobject.transform.rotation);
        
            Destroy (m_GhostBuildGameobject);
            m_GhostBuildGameobject = null;

            m_IsBuilding = false;

            foreach (Connector connector in newBuild.GetComponentsInChildren<Connector>())
            {
                connector.UpdateConnectors(true);
            }
        }
    }

    private void GhostDestroy()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.root.CompareTag("Buildables"))
            {
                if (!m_LastHitDestroyTransform)
                {
                    m_LastHitDestroyTransform = hit.transform.root;

                    m_LastHitMaterialList.Clear();
                    foreach (MeshRenderer lastHitMeshRenderer in m_LastHitDestroyTransform.GetComponentsInChildren<MeshRenderer>())
                    {
                        m_LastHitMaterialList.Add(lastHitMeshRenderer.material);
                    }

                    GhostifyModel(m_LastHitDestroyTransform.GetChild(0), m_GhostMaterialInvalid);
                }
                else if (hit.transform.root != m_LastHitDestroyTransform)
                {
                    ResetLastHitDestroyTransform();
                }
            }
            else if (m_LastHitDestroyTransform)
            {
                ResetLastHitDestroyTransform();
            }
        }
    }

    private void ResetLastHitDestroyTransform()
    {
        int counter = 0;
        
        foreach (MeshRenderer lastHitMeshRenderer in m_LastHitDestroyTransform.GetComponentsInChildren<MeshRenderer>())
        {
            lastHitMeshRenderer.material = m_LastHitMaterialList[counter];
            counter++;
        }

        m_LastHitDestroyTransform = null;
    }

    private void DestroyBuild()
    {
        if (m_LastHitDestroyTransform)
        {
            foreach (Connector connector in m_LastHitDestroyTransform.GetComponentsInChildren<Connector>())
            {
                connector.gameObject.SetActive(false);
                connector.UpdateConnectors(true);
            }

            Destroy(m_LastHitDestroyTransform.gameObject);

            m_IsDestroying = false; 
            m_LastHitDestroyTransform = null;
        }
    }
}

[System.Serializable]
public enum SelectedBuildType
{
    floor,
    wall,
    Cube1,
    Cube2
}
