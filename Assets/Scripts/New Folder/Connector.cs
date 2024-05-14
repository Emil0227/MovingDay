using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public ConnectorPosition ConnectorPos;
    public SelectedBuildType ConnectorParentType;

    [HideInInspector] public bool IsConnectedToFloor = false;
    [HideInInspector] public bool IsConnectedToWall = false;
    [HideInInspector] public bool CanConnectTo = true;
    
    [SerializeField] private bool m_CanConnectToFloor = true;
    [SerializeField] private bool m_CanConnectToWall = true;


    private void OnDrawGizmos()
    {
        Gizmos.color = IsConnectedToFloor ? (IsConnectedToWall ? Color.red : Color.blue) : (!IsConnectedToWall ? Color.green : Color.yellow);
        Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x / 2f);
    }

    public void UpdateConnectors(bool rootCall = false)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.lossyScale.x / 2f);
        IsConnectedToFloor = !m_CanConnectToFloor;
        IsConnectedToWall = !m_CanConnectToWall;

        foreach (Collider collider in colliders)
        {
            if (collider.GetInstanceID() == GetComponent<Collider>().GetInstanceID())
            {
                continue;
            }

            if (!collider.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (collider.gameObject.layer == gameObject.layer)
            {
                Connector foundConnector = collider.GetComponent<Connector>();

                if (foundConnector.ConnectorParentType == SelectedBuildType.floor)
                    IsConnectedToFloor = true;
               
                if (foundConnector.ConnectorParentType == SelectedBuildType.wall)
                    IsConnectedToWall = true;

                if (rootCall)
                    foundConnector.UpdateConnectors();
            }
        }

        CanConnectTo = true;

        if (IsConnectedToFloor && IsConnectedToWall)
            CanConnectTo = false;
    }

    [System.Serializable]
    public enum ConnectorPosition
    {
        Left,
        Right,
        Top,
        Bottom,
        Back,
        Front
    }
}
