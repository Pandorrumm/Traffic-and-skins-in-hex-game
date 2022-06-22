using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GameLogic;
using GameEntity;
using System;
using static GameData.HexSideData;
using Singleton;


public class Traffic : MonoBehaviour
{
    [SerializeField] private float speed = 0;

    public List<Vector3> wayPoints = new List<Vector3>();
    public List<Vector3> wayPointToMove = new List<Vector3>();
    private List<Vector3> forCopy = new List<Vector3>();

    private int indexWayPoint;
    public bool isMoveTransport = false;

    [HideInInspector]
    public HexOfWorld firstHex;
    [HideInInspector]
    public HexOfWorld lastHex;

    private ChainDetection chainDetection;

    private int numberHexInTransportChain;

    public static Action<string, List<Vector3>> SaveWayPointsEvent;

    private void OnEnable()
    {
        ChainDetection.MoveObjectEvent += CheckingBeginningOrEndHexesChain;
        SaveLoadFreeGame.AssigningFirstAndLastHexesAfterLoadingGameEvent += AssigningFirstAndLastHexesAfterLoadingGame;
        ChainDetection.RemovingHexPositionFromWayPointEvent += RemovingHexPositionFromWayPoint;

        chainDetection = FindObjectOfType<ChainDetection>();
    }

    private void OnDisable()
    {
        ChainDetection.MoveObjectEvent -= CheckingBeginningOrEndHexesChain;
        SaveLoadFreeGame.AssigningFirstAndLastHexesAfterLoadingGameEvent -= AssigningFirstAndLastHexesAfterLoadingGame;
        ChainDetection.RemovingHexPositionFromWayPointEvent -= RemovingHexPositionFromWayPoint;
    }

    private void Update()
    {
        if (isMoveTransport)
        {
            Move();
        }
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, wayPointToMove[indexWayPoint], speed * Time.deltaTime);
        transform.LookAt(wayPointToMove[indexWayPoint]);

        if (transform.position == wayPointToMove[indexWayPoint])
        {
            indexWayPoint++;
        }

        if (indexWayPoint == wayPointToMove.Count)
        {
            isMoveTransport = false;
            // indexWayPoint = 0;

            BuildingWayOfMovement();
        }
    }

    public void BuildingWayOfMovement()
    {
        wayPointToMove.Clear();
        forCopy.Clear();

        wayPointToMove.AddRange(wayPoints);
        forCopy.AddRange(wayPointToMove);

        forCopy.Reverse();
        forCopy.RemoveAt(0);

        wayPointToMove.AddRange(forCopy);

        indexWayPoint = wayPointToMove.IndexOf(transform.position);
        isMoveTransport = true;
    }

    private void CheckingBeginningOrEndHexesChain(GameObject _transport, HexOfWorld _currentHex, SideType _sideType)
    {
        if (gameObject == _transport)
        {
            if (firstHex == null)
            {
                firstHex = _currentHex;
            }
            else if (lastHex == null)
            {
                lastHex = _currentHex;
            }

            if (wayPoints.Count < 2)
            {
                wayPoints.Insert(0, GetTransformPosition(_currentHex));
                forCopy.Insert(0, GetTransformPosition(_currentHex));

                wayPointToMove.Clear();
                wayPointToMove.AddRange(forCopy);

                isMoveTransport = true;
            }
            else
            {
                if (firstHex != null)
                {
                    foreach (KeyValuePair<HexOfWorld.Side, HexOfWorld> keyValue in firstHex.neightbours)
                    {
                        if (keyValue.Value != null && keyValue.Value.Equals(_currentHex))
                        {
                            if (firstHex.data.sidesData[keyValue.Key].sideData.type == _sideType)
                            {
                                firstHex = _currentHex;
                                wayPoints.Insert(wayPoints.Count, GetTransformPosition(firstHex));
                            }
                        }
                    }
                }

                if (lastHex != null)
                {
                    foreach (KeyValuePair<HexOfWorld.Side, HexOfWorld> keyValue in lastHex.neightbours)
                    {
                        if (keyValue.Value != null && keyValue.Value.Equals(_currentHex))
                        {
                            if (lastHex.data.sidesData[keyValue.Key].sideData.type == _sideType)
                            {
                                lastHex = _currentHex;
                                wayPoints.Insert(0, GetTransformPosition(lastHex));
                            }
                        }
                    }
                }
            }

            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
            {
                SaveWayPointsEvent?.Invoke(gameObject.name, wayPoints);
            }

            forCopy.Clear();
            forCopy.AddRange(wayPoints);
        }
    }

    private Vector3 GetTransformPosition(HexOfWorld _hex)
    {
        Vector3 wayPointPosition = _hex.GetComponentInChildren<WayPoint>().transform.position;

        return wayPointPosition;
    }

    public bool isThisWayPointIsOnTheWay(HexOfWorld _hex)
    {
        return wayPoints.Contains(_hex.GetComponentInChildren<WayPoint>().transform.position);
    }

    private void AssigningFirstAndLastHexesAfterLoadingGame(HexOfWorld _hex)
    {
        if (firstHex == null)
        {
            firstHex = _hex;
          //  Debug.Log(firstHex.name, firstHex);
        }
        else if (lastHex == null)
        {         
            lastHex = _hex;
           // Debug.Log(lastHex.name, lastHex);
        }
    }

    private void RemovingHexPositionFromWayPoint(HexOfWorld _deletedHex)
    {
        if (isThisWayPointIsOnTheWay(_deletedHex))
        {
            isMoveTransport = false;

            gameObject.SetActive(false);

            firstHex = null;
            lastHex = null;

            int indexDeletedHexWayPointsPosition = wayPoints.IndexOf(_deletedHex.GetComponentInChildren<WayPoint>().transform.position);
            wayPoints.Remove(_deletedHex.GetComponentInChildren<WayPoint>().transform.position);

            HexOfWorld hexOnWhichTransport = chainDetection.HexSearchByWayPoint(wayPoints[0]);
                     
            DOTween.Sequence()
                           .AppendInterval(0.15f)
                           .AppendCallback(() => AssignNewWayPoint());

            void AssignNewWayPoint()
            {
                numberHexInTransportChain = chainDetection.GetChainCountByHex(hexOnWhichTransport);

               // Debug.Log("Кол-во гексов в транспортной цепи " + numberHexInTransportChain);

                wayPointToMove.Clear();

                if (numberHexInTransportChain > 0)
                {
                    for (int i = wayPoints.Count - 1; i >= numberHexInTransportChain; i--)
                    {
                        wayPoints.RemoveAt(i);
                    }

                    ReassigningWayPointToNewChain(hexOnWhichTransport);
                }
                else if (numberHexInTransportChain == 0)
                {
                    for (int i = 0; i < indexDeletedHexWayPointsPosition; i++)
                    {
                        wayPoints.RemoveAt(i);
                    }

                    ReassigningWayPointToNewChain(hexOnWhichTransport);
                }
            }
        }
    }

    private void ReassigningWayPointToNewChain(HexOfWorld _hexOnWhichTransport)
    {
        if (wayPoints.Count > 1)
        {
            transform.position = wayPoints[0];

            gameObject.SetActive(true);

            HexOfWorld lastHexInWayPoint = chainDetection.HexSearchByWayPoint(wayPoints[wayPoints.Count - 1]);

            AssigningFirstAndLastHexesAfterLoadingGame(lastHexInWayPoint);
            AssigningFirstAndLastHexesAfterLoadingGame(_hexOnWhichTransport);

            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
            {
                SaveWayPointsEvent?.Invoke(gameObject.name, wayPoints);
            }

            BuildingWayOfMovement();
        }
        else
        {
            DestroyTransport();
        }
    }

    private void DestroyTransport()
    {
        if (chainDetection.cars.Contains(gameObject))
        {
            chainDetection.cars.Remove(gameObject);
        }
        else if (chainDetection.boats.Contains(gameObject))
        {
            chainDetection.boats.Remove(gameObject);
        }
        else if (chainDetection.trains.Contains(gameObject))
        {
            chainDetection.trains.Remove(gameObject);
        }

        Destroy(gameObject); 
    }
}

