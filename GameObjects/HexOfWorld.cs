using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameData;
using TMPro;
using Sirenix.OdinInspector;
using System;
using Helpers;
using Singleton;
using DG.Tweening;
using UnityEngine.UI;
using InputSystem;
using GameUI;
using GameLogic;
using static GameData.HexSideData;

namespace GameEntity
{
    public class HexOfWorld : SerializedMonoBehaviour, IInteractble
    {
        [BoxGroup("Render"), OnValueChanged("UpdateTexts")]
        public bool drawTextNames;
        [BoxGroup("Render"), ShowIf("drawTextNames")]
        public Dictionary<Side, TextMeshProUGUI> textTitles;

        [BoxGroup("Logic")]
        public Dictionary<Side, HexZoneToBuild> emptyCells;
        [BoxGroup("Logic")]
        public Dictionary<Side, Transform> spawnPoints;
        [BoxGroup("Logic"), ReadOnly]
        public Dictionary<Side, HexOfWorld> neightbours = new Dictionary<Side, HexOfWorld>();

        [OnValueChanged("Init")]
        public CellOfWorldDataConfig startData;
        [ReadOnly]
        public CellOfWorldData data;

        private int _currentDataAngle = 0;
        public int currentDataAngle {
            set {
                _currentDataAngle = value;
                if(data != null) {
                    data.currentAngle = _currentDataAngle;
                }
            }
            get {
                return _currentDataAngle;
            }
        }

        public Transform modelContainer;
        public GameObject hexFrame;
        [Space]
        public HexOfWorld prefubCellOfWorld;
        [Space]
        [HideInInspector]
        public List<ParticleSystem> effectsHex = new List<ParticleSystem>();
        [Space]
        [Header("ParticleSystem Weather Conditions")]
        public ParticleSystem effectAutumn;
        public ParticleSystem effectSummer;
        private bool isTheWeatherHasBeenCreated = false;
        [Space]
        public GameObject spawnPointWeatherConditions;
        [Space]
        public LayerMask cellOfWorldMask;
        public LayerMask cellToBuildMask;

        private BuildingCellDetection buildingCellDetection;

        [HideInInspector]
        public Action OnFinishLoading;
        [HideInInspector]
        public Action OnChangeModel;
        [HideInInspector]
        public Action<Quaternion> OnFinishRotation;

        private GameObject _go;
        public GameObject go {
            get { return gameObject; }
            set { _go = value; }
        }

        private bool isRotating = false;
        private float rotateSpeed = 0.2f;

        private HexModel hexModel;

        [BoxGroup("Quest")]
        public bool isQuestHex = false;
        [BoxGroup("Quest"), ShowIf("isQuestHex")]
        public QuestHexDataConfig startQuestConfig;
        [BoxGroup("Quest"), ShowIf("isQuestHex")]
        public TextMeshProUGUI questText;
        [BoxGroup("Quest"), ShowIf("isQuestHex")]
        public Image questIcon;
        [BoxGroup("Quest"), ShowIf("isQuestHex"), ReadOnly]
        public HexQuestData questData;

        [SerializeField]
        private List<MaterialColor> materials = new List<MaterialColor>();
        private Coroutine coroutineMaterailsBlinking;

        private Overlay overlay;

        public static Action EnableButtonInTutorialEvent;
        public static Action UpdateHexTaskUIEvent;

        private void Awake()
        {
            buildingCellDetection = FindObjectOfType<BuildingCellDetection>();
            overlay = FindObjectOfType<Overlay>();

            GameMode.OnChangeMode += GameModeWasChanged;

            BuildingCellDetection.OnHexMoveOtherPlace += OnPointerLeftDragAndDropZone;
            BuildingCellDetection.OnGhostHexPlaced += OnGhostHexPlased;

            UIDragAndDropZone.OnPointerExitZone += OnPointerLeftDragAndDropZone;
            UIDragAndDropZone.OnPointerEnterZone += OnPointerEnterDragAndDropZone;
            UIDragAndDropZone.OnDrugAndDropRelesed += OnDrugAndDropRelesed;

            UpdateTexts();
            Init();
        }

        private void OnGhostHexPlased(HexOfWorld obj)
        {
            StopBlinking();
        }

        private void OnDrugAndDropRelesed(Vector2 pos)
        {
            StopBlinking();
        }

        private void UpdateTexts()
        {
            if (drawTextNames) {
                foreach (KeyValuePair<Side, TextMeshProUGUI> entry in textTitles) {
                    if(entry.Value != null && entry.Value.GetComponentInParent<Canvas>() != null)
                        entry.Value.GetComponentInParent<Canvas>().gameObject.SetActive(true);
                }
            } else {
                foreach (KeyValuePair<Side, TextMeshProUGUI> entry in textTitles) {
                    if (entry.Value != null && entry.Value.GetComponentInParent<Canvas>() != null)
                        entry.Value.GetComponentInParent<Canvas>().gameObject.SetActive(false);
                }
            }
        }

        private void OnButtonOkayPressed()
        {
            BuildingCellDetection cellDetection = FindObjectOfType<BuildingCellDetection>();

            if(cellDetection != null) {
                cellDetection.OnOkay();
            }
        }

        private void OnDestroy()
        {
            GameMode.OnChangeMode -= GameModeWasChanged;

            BuildingCellDetection.OnHexMoveOtherPlace -= OnPointerLeftDragAndDropZone;
            BuildingCellDetection.OnGhostHexPlaced -= OnGhostHexPlased;

            UIDragAndDropZone.OnPointerExitZone -= OnPointerLeftDragAndDropZone;
            UIDragAndDropZone.OnPointerEnterZone -= OnPointerEnterDragAndDropZone;
            UIDragAndDropZone.OnDrugAndDropRelesed -= OnDrugAndDropRelesed;
        }

        public void Init()
        {
            data = startData.data.Copy();
            data.gameObject = gameObject;
            isQuestHex = false;

            UpdateText();
            modelContainer.transform.Clear();

            GameObject model = Instantiate(data.prefub, modelContainer);

            hexModel = model.GetComponent<HexModel>();

            if (hexModel != null)
            {
                hexModel.UpdateSides(data);
            }

           // UpdateMaterials();          

            turnOffAllPlaceZones();

            if (OnFinishLoading != null) OnFinishLoading();
        }

        //public void StartBlinking(CellOfWorldData recipeResult)
        //{
        //   // coroutineMaterailsBlinking = StartCoroutine(cr_blink(recipeResult));
        //}

        private IEnumerator cr_blink(CellOfWorldData recipeResult)
        {
            while (true) {
                float currentTime = 0;
                float cycleTyme = 1f;
                Color currentColor;

                while (true) {
                    currentTime += Time.deltaTime;

                    //выдавал ошибку по материалу, работает на исходнике
                    //for(int i = 0; i < materials.Count; i++) {
                    //    currentColor = Color.Lerp(materials[i].startColor, 
                    //                              recipeResult.sidesData[hexModel.GetCorrectSide(i)].sideData.textColor, 
                    //                              currentTime / cycleTyme);
                    //    materials[i].mat.SetColor("_Color", currentColor);
                    //}

                    //foreach (MaterialColor matColor in materials) {
                    //    currentColor = Color.Lerp(matColor.startColor, Color.white, currentTime / cycleTyme);
                    //    matColor.mat.SetColor("_Color", currentColor);
                    //}

                    if (currentTime >= cycleTyme) {
                        currentTime = 0;
                        break;
                    }

                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);

                while (true) {
                    currentTime += Time.deltaTime;

                    for (int i = 0; i < materials.Count; i++) {
                        currentColor = Color.Lerp(recipeResult.sidesData[hexModel.GetCorrectSide(i)].sideData.textColor, 
                                                  materials[i].startColor, 
                                                  currentTime / cycleTyme);
                        materials[i].mat.SetColor("_Color", currentColor);
                    }

                    //foreach (MaterialColor matColor in materials) {
                    //    currentColor = Color.Lerp(Color.white, matColor.startColor, currentTime / cycleTyme);
                    //    matColor.mat.SetColor("_Color", currentColor);
                    //    //matColor.mat.color = currentColor;
                    //}

                    if (currentTime >= cycleTyme) {
                        currentTime = 0;
                        break;
                    }

                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);
            }
        }

        public void StopBlinking()
        {
            if (coroutineMaterailsBlinking != null) {
                StopCoroutine(coroutineMaterailsBlinking);
                coroutineMaterailsBlinking = null;
            }

            foreach(MaterialColor matColor in materials) {
                matColor.mat.color = matColor.startColor;
            }
        }

        private void UpdateMaterials()
        {
            MaterialColor matColor;
            if (hexModel != null)
            {
                for (int i = 0; i < hexModel.meshRenderer.materials.Length; i++)
                {
                    matColor = new MaterialColor();
                    matColor.mat = hexModel.meshRenderer.materials[i];
                    matColor.startColor = hexModel.meshRenderer.materials[i].color;

                    materials.Add(matColor);
                }
            }
        }

        public void ChangeModel(CellOfWorldData data, Quaternion rotation)
        {
            UpdateData(data);

            if (hexModel != null)
            {
                hexModel.UpdateSides(data);
            } 
            else 
            {
                modelContainer.transform.Clear();
                GameObject model = Instantiate(data.prefub, modelContainer);
                model.transform.rotation = rotation;
                hexModel = model.GetComponent<HexModel>();

                if (hexModel != null)
                {
                    hexModel.UpdateSides(data);
                }               
            }

            if (hexModel != null)
            {
                materials.Clear();

                UpdateMaterials();
            }

            if (OnChangeModel != null) OnChangeModel();
        }

        public void UpdateData(CellOfWorldData data)
        {
            this.data = data.Copy();
            UpdateText();
            UpdateQuestText();
        }

        public void MakeQuestHex(QuestHexDataConfig questConfig)
        {
            if (questConfig != null) {
                questText.transform.parent.gameObject.SetActive(true);
                isQuestHex = true;
                startQuestConfig = questConfig;

                questData = startQuestConfig.questData.Copy();

                UpdateQuestText();
            } else {
                Debug.LogError("questConfig == null");
            }
        }

        public void MakeNormalHex()
        {
            isQuestHex = false;
            startQuestConfig = null;
            questData = null;

            questText.text = "";
            questText.transform.parent.gameObject.SetActive(false);
        }

        public void UpdateQuestText()
        {
            if (questText != null)
            {
                if (isQuestHex)
                {
                    questText.transform.parent.gameObject.SetActive(true);

                    //Изменение цвета текса о твыполнения квеста

                    if (isQuestScoreLowerThenNeed())
                    {
                        questText.color = new Color(0.133f, 0.090f, 0.384f, 1);
                        // questText.color = new Color(questText.color.r, questText.color.g, questText.color.b, 1);
                    }
                    else if (isQuestScoreCorrect())
                    {
                        questText.color = Color.green;
                    }
                    else if (isQuestScoreHigherThenNeed())
                    {
                        questText.color = Color.red;
                    }

                    questText.text = questData.currentScore + "/" + questData.requireScore;
                    questIcon.sprite = questData.questIcon;

                    DOTween.Sequence()
                           .AppendInterval(0.3f)
                           .AppendCallback(() => UpdateHexTaskUIEvent?.Invoke());
                    
                }
                else
                {
                    questText.transform.parent.gameObject.SetActive(false);
                    questText.text = "";
                }
            }
        }

        public void UpdateText()
        {
            for (int i = 0; i < Enum.GetValues(typeof(Side)).Length; i++) {
                textTitles[(Side)i].text = data.sidesData[(Side)i].sideData.localization;
                textTitles[(Side)i].color = data.sidesData[(Side)i].sideData.textColor;
            }
        }

        public bool isContainsSide(SideType sideType)
        {
            foreach (KeyValuePair<Side, SideDataAndConditions> a in data.sidesData) {
                if (a.Value != null && a.Value.sideData.type == sideType) return true;
            }

            return false;
        }

        public void GameModeWasChanged(GameMode.Mode mode)
        {
            switch (mode) {
                case GameMode.Mode.DrugAndDropStart:
                    // Nothing
                    break;
                case GameMode.Mode.DrugAndDropRelesed:
                case GameMode.Mode.CameraControl:
                case GameMode.Mode.EditCell:
                    turnOffAllPlaceZones();
                    break;
            }
        }

        private void OnPointerLeftDragAndDropZone(bool isPressed)
        {
            if (isPressed) {
                turnOnAllPlaceZones();

                if (overlay.isLayout.isOn) {
                    // Включить мерцание если хекс драг н дропа и текущий могут слиться
                    CellOfWorldData recipeResult =
                        overlay.getResult(data, SingletonHexCollection.Instance.GetCurentHexDatafromPreview());

                    if (recipeResult != null) {
                        Debug.Log(recipeResult.localizedName);

                        //StartBlinking(recipeResult);
                    }
                }
            }
        }

        private void OnPointerEnterDragAndDropZone(bool isPressed)
        {
            if (isPressed) {
                turnOffAllPlaceZones();

                //if (overlay.isLayout.isOn) {
                    // Остановить мерцание
                StopBlinking();
                //}
            }
        }

        private bool isLastCreatedWorldCell()
        {
            return BuildingCellDetection.lastCreatedCell != null &&
                        this.Equals(BuildingCellDetection.lastCreatedCell);
        }

        public void RotateLeft()
        {           
            if (!isRotating)
            {
                SideDataAndConditions tmp = data.sidesData[Side.BottomLeft];

                data.sidesData[Side.BottomLeft] = data.sidesData[Side.BottomRight];
                data.sidesData[Side.BottomRight] = data.sidesData[Side.Right];
                data.sidesData[Side.Right] = data.sidesData[Side.TopRight];
                data.sidesData[Side.TopRight] = data.sidesData[Side.TopLeft];
                data.sidesData[Side.TopLeft] = data.sidesData[Side.Left];
                data.sidesData[Side.Left] = tmp;

                currentDataAngle += 60;

                if (currentDataAngle == 360)
                    currentDataAngle = 0;

                SoundsAndMusicController.Instance.PlayPlace();
              
                RotateModelLeft();
            }
        }

        public void RotateLeftNewHexInFreeGame(int _numberOfTimes)
        {
            for (int i = 0; i < _numberOfTimes; i++)
            {
                SideDataAndConditions tmp = data.sidesData[Side.BottomLeft];

                data.sidesData[Side.BottomLeft] = data.sidesData[Side.BottomRight];
                data.sidesData[Side.BottomRight] = data.sidesData[Side.Right];
                data.sidesData[Side.Right] = data.sidesData[Side.TopRight];
                data.sidesData[Side.TopRight] = data.sidesData[Side.TopLeft];
                data.sidesData[Side.TopLeft] = data.sidesData[Side.Left];
                data.sidesData[Side.Left] = tmp;

                currentDataAngle += 60;

                if (currentDataAngle == 360)
                    currentDataAngle = 0;
            }
        }

        public void RotateModelLeft()
        {
            if (!isRotating && gameObject.activeSelf)
            {
                StartCoroutine(cr_WaitRotatingAnimationFinish());

                Vector3 newRotation = new Vector3(0, 0, 60f);

                if (modelContainer.GetChild(0).transform != null)
                {
                    modelContainer.GetChild(0).transform.DORotate(newRotation, rotateSpeed, RotateMode.LocalAxisAdd);

                    if (Tutorial.Instance.isTutorial && Tutorial.Instance.currentScreen < 2)
                    {
                        EnableButtonInTutorialEvent?.Invoke();
                    }
                }
            }
        }

        public void RotateRight()
        {          
            if (!isRotating) {
                SideDataAndConditions tmp = data.sidesData[Side.BottomLeft];

                data.sidesData[Side.BottomLeft] = data.sidesData[Side.Left];
                data.sidesData[Side.Left] = data.sidesData[Side.TopLeft];
                data.sidesData[Side.TopLeft] = data.sidesData[Side.TopRight];
                data.sidesData[Side.TopRight] = data.sidesData[Side.Right];
                data.sidesData[Side.Right] = data.sidesData[Side.BottomRight];
                data.sidesData[Side.BottomRight] = tmp;

                currentDataAngle -= 60;

                if (currentDataAngle == -60)
                    currentDataAngle = 300;

                SoundsAndMusicController.Instance.PlayPlace();

                RotateModelRight();
            }
        }

        [Button("Set Angle 60"), ButtonGroup("Set Angle")]
        public void SetAngle60()
        {
            SetDataAngle(60, startData.data);
        }

        [Button("Set Angle 120"), ButtonGroup("Set Angle")]
        public void SetAngle120()
        {
            SetDataAngle(120, startData.data);
        }

        [Button("Set Angle 180"), ButtonGroup("Set Angle")]
        public void SetAngle180()
        {
            SetDataAngle(180, startData.data);
        }

        [Button("Set Angle 240"), ButtonGroup("Set Angle")]
        public void SetAngle240()
        {
            SetDataAngle(240, startData.data);
        }

        [Button("Set Angle 300"), ButtonGroup("Set Angle")]
        public void SetAngle300()
        {
            SetDataAngle(300, startData.data);
        }

        [Button("Set Angle 360"), ButtonGroup("Set Angle")]
        public void SetAngle360()
        {
            SetDataAngle(0, startData.data);
        }

        public void SetDataAngle(int angle, CellOfWorldData startData)
        {
            int rotateLeftAmount = angle / 60;

            data = startData.Copy();

            if (rotateLeftAmount > 0) {
                for (int i = 0; i < rotateLeftAmount; i++) {
                    SideDataAndConditions tmp = data.sidesData[Side.BottomLeft];

                    data.sidesData[Side.BottomLeft] = data.sidesData[Side.BottomRight];
                    data.sidesData[Side.BottomRight] = data.sidesData[Side.Right];
                    data.sidesData[Side.Right] = data.sidesData[Side.TopRight];
                    data.sidesData[Side.TopRight] = data.sidesData[Side.TopLeft];
                    data.sidesData[Side.TopLeft] = data.sidesData[Side.Left];
                    data.sidesData[Side.Left] = tmp;
                }

                UpdateText();
                UpdateTexts();

                currentDataAngle = angle;

                Vector3 newRotation = new Vector3(0, 0, angle);

                Transform hexagonModelTransform = modelContainer.GetChild(0).transform;

                if (hexagonModelTransform != null) {
                    hexagonModelTransform.localRotation = 
                        Quaternion.Euler(hexagonModelTransform.localEulerAngles.x, 
                                         hexagonModelTransform.localEulerAngles.y,
                                         angle);

                    Debug.Log("Change angle to " + angle + "hexagonModelTransform rotation: " + hexagonModelTransform.localRotation.eulerAngles);

                    //hexagonModelTransform.DOLocalRotate(newRotation, 0.01f, RotateMode.Fast);
                }
            } else {
                UpdateText();
                UpdateTexts();

                currentDataAngle = angle;

                Transform hexagonModelTransform = modelContainer.GetChild(0).transform;

                if (hexagonModelTransform != null) {
                    hexagonModelTransform.localRotation =
                        Quaternion.Euler(hexagonModelTransform.localEulerAngles.x,
                                         hexagonModelTransform.localEulerAngles.y,
                                         0);
                }
            }
        }

        public void RotateModelRight()
        {
            if (!isRotating) {
                StartCoroutine(cr_WaitRotatingAnimationFinish());

                Vector3 newRotation = new Vector3(0, 0, -60f);

                if (modelContainer.GetChild(0).transform != null) {
                    modelContainer.GetChild(0).transform.DORotate(newRotation, rotateSpeed, RotateMode.LocalAxisAdd);
                }
            }
        }

        private IEnumerator cr_WaitRotatingAnimationFinish()
        {
            isRotating = true;
            yield return new WaitForSeconds(rotateSpeed);
            isRotating = false;

            yield return new WaitForFixedUpdate();
            UpdateNeightbours();
            UpdateText();

            buildingCellDetection.UpdatePlaceButton();

            //if (isAllConnectionsOkayWithNeightbours() == false) {
            //    RotateLeft();
            //}

            if (OnFinishRotation != null) OnFinishRotation(transform.rotation);
        }

        public bool isPlaceFree(Side side)
        {
            Collider[] colliders = Physics.OverlapSphere(spawnPoints[side].position, 0.1f);

            return colliders.Length == 0;
        }

        public bool isPlaceFreeFromWorldCell(Side side)
        {
            List<Collider> result = new List<Collider>(Physics.OverlapSphere(spawnPoints[side].position, 0.1f, cellOfWorldMask));

            //foreach (Collider col in result) {
            //    if (col.gameObject == this.gameObject) {
            //        result.Remove(col);
            //    }
            //}
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].gameObject == this.gameObject)
                {
                    result.Remove(result[i]);
                }
            }
                return result.Count == 0;
        }

        public Collider[] getWorldCellColliders(Side side)
        {
            List<Collider> result = new List<Collider>(Physics.OverlapSphere(spawnPoints[side].position, 0.1f, cellOfWorldMask));

            foreach(Collider col in result) {
                if(col.gameObject == this.gameObject) {
                    result.Remove(col);
                }
            }

            return result.ToArray();
        }

        [Button("TopLeft"), ButtonGroup("CheckFreePlace")]
        public void isPlaceFreeFromWorldCellTopLeft()
        {
            Debug.Log(isPlaceFreeFromWorldCell(Side.TopLeft));
        }

        [Button("TopRight"), ButtonGroup("CheckFreePlace")]
        public void isPlaceFreeFromWorldCellTopRight()
        {
            Debug.Log(isPlaceFreeFromWorldCell(Side.TopRight));
        }

        [Button("Left"), ButtonGroup("CheckFreePlace")]
        public void isPlaceFreeFromWorldCellLeft()
        {
            Debug.Log(isPlaceFreeFromWorldCell(Side.Left));
        }

        [Button("Right"), ButtonGroup("CheckFreePlace")]
        public void isPlaceFreeFromWorldCellRight()
        {
            Debug.Log(isPlaceFreeFromWorldCell(Side.Right));
        }

        [Button("BottomLeft"), ButtonGroup("CheckFreePlace")]
        public void isPlaceFreeFromWorldCellBottomLeft()
        {
            Debug.Log(isPlaceFreeFromWorldCell(Side.BottomLeft));
        }

        [Button("BottomRight"), ButtonGroup("CheckFreePlace")]
        public void isPlaceFreeFromWorldCellBottomRight()
        {
            Debug.Log(isPlaceFreeFromWorldCell(Side.BottomRight));
        }

        public bool isPlaceZoneSelf(Side side, HexZoneToBuild cellToBuild)
        {
            return emptyCells[side] == cellToBuild;
        }

        public bool isQuestScoreLowerThenNeed()
        {
            return questData.currentScore < questData.requireScore;
        }

        public bool isQuestScoreCorrect()
        {
            return questData.currentScore == questData.requireScore;
        }

        public bool isQuestScoreHigherThenNeed()
        {
            return questData.currentScore > questData.requireScore;
        }

        public bool isHexNeightbourOfHex(HexOfWorld hex)
        {
            foreach (KeyValuePair<Side, HexOfWorld> keyValue in neightbours) {
                if(keyValue.Value != null && keyValue.Value.Equals(hex)) {
                    return true;
                }
            }

            return false;
        }

        public bool isAllConnectionsOkayWithNeightbours(CellOfWorldData data)
        {
            int counterMatches = 0;
            
            foreach (Side side in (Side[])Enum.GetValues(typeof(Side))) {
                // Проверка связи стороны текущего гексагона
                if (isSideHasAnyConnectors(side, data)) {
                    if (isNeightbourNotNull(side)) {
                        int count = 0;

                        for (int i = 0; i < data.sidesData[side].connectors.Count; i++) {
                            if (isNeightbourHasCorrectSideWithOurSide(side, SideHelper.GetAdjacedSide(side), i, data)) {
                                counterMatches++;
                                count++;
                                break;
                            }
                        }

                        if (count == 0) return false;
                    } else {
                        counterMatches++;
                    }
                } else {
                    counterMatches++;
                }

                // Проверка стороны соседнего хексагона
                if (isNeightbourSideHasAnyConnectors(side)) {
                    List<HexSideData> connectors =
                        neightbours[side].data.sidesData[SideHelper.GetAdjacedSide(side)].connectors;

                    int neightbourSideCounter = 0;

                    foreach (HexSideData hexSideData in connectors) {
                        if (hexSideData.type == data.sidesData[side].sideData.type) {
                            neightbourSideCounter++;
                        }
                    }

                    if (neightbourSideCounter == 0) {
                        return false;
                    }
                }
            }

            if (counterMatches == 0) {
                return false;
            } else if (counterMatches > 0) {
                return true;
            }

            return true;
        }

        public bool isAllConnectionsOkayWithNeightbours()
        {
            return isAllConnectionsOkayWithNeightbours(data);
        }

        private bool isNeightbourNotNull(Side side)
        {
            return neightbours.ContainsKey(side) && neightbours[side] != null;
        }

        private bool isNeightbourSideHasAnyConnectors(Side side)
        {
            return neightbours[side] != null &&
                           neightbours[side].data.sidesData[SideHelper.GetAdjacedSide(side)] != null &&
                           neightbours[side].data.sidesData[SideHelper.GetAdjacedSide(side)].connectors != null &&
                           neightbours[side].data.sidesData[SideHelper.GetAdjacedSide(side)].connectors.Count > 0;
        }

        private bool isSideHasAnyConnectors(Side side, CellOfWorldData data)
        {
            return data.sidesData.ContainsKey(side) &&
                   data.sidesData[side].connectors != null &&
                   data.sidesData[side].connectors.Count > 0;
        }

        private bool isNeightbourHasCorrectSideWithOurSide(Side currentSide, Side neightbourSide, int iterator, CellOfWorldData data)
        {
            return neightbours.ContainsKey(currentSide) &&
                                   neightbours[currentSide] != null &&
                                   neightbours[currentSide].data.sidesData[neightbourSide].sideData.type ==
                                   data.sidesData[currentSide].connectors[iterator].type;
        }

        public bool isHasFreeSide()
        {   
            foreach (KeyValuePair<Side, HexOfWorld> keyValue in neightbours)
            {
                if (keyValue.Value == null)
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateQuestScore(int currentScore)
        {
            questData.currentScore = currentScore;
            UpdateQuestText();
        }

        public void changePlaceZoneState(Side side, bool state)
        {
            if (emptyCells[side] != null) {
                Border border = FindObjectOfType<Border>();

                if (border != null) {
                    if (isPlaceFreeFromWorldCell(side) && border.isPositionInsideBorder(emptyCells[side].transform.position)) {
                        emptyCells[side].gameObject.SetActive(state);
                    } else {
                        emptyCells[side].gameObject.SetActive(false);
                    }
                } else if(border == null) {
                    if (isPlaceFreeFromWorldCell(side)) {
                        emptyCells[side].gameObject.SetActive(state);
                    } else {
                        emptyCells[side].gameObject.SetActive(false);
                    }
                }
            }
        }

        public void turnOffAllPlaceZones()
        {
            for (int i = 0; i < Enum.GetValues(typeof(Side)).Length; i++) {
                changePlaceZoneState((Side)i, false);
            }
        }

        public void turnOnAllPlaceZones()
        {
            if ((buildingCellDetection != null &&
                BuildingCellDetection.lastCreatedCell != null &&
                this != BuildingCellDetection.lastCreatedCell) ||
                (buildingCellDetection == null || BuildingCellDetection.lastCreatedCell == null)) {
                for (int i = 0; i < Enum.GetValues(typeof(Side)).Length; i++) {
                    changePlaceZoneState((Side)i, true);
                }
            }
        }

        public HexOfWorld SpawnCellOfWorld(Side side, Quaternion rotation)
        {
            changePlaceZoneState(side, false);
            if (isPlaceFreeFromWorldCell(side)) {
                //GameObject go = Instantiate(prefubCellOfWorld.gameObject, spawnPoints[side].position, 
                //                           prefubCellOfWorld.gameObject.transform.rotation);

                GameObject go = Instantiate(prefubCellOfWorld.gameObject, spawnPoints[side].position,
                                            rotation);

               // Debug.Log(go.transform.position + " - " + spawnPoints[side].position);

                HexOfWorld newCellOfWorld = go.GetComponent<HexOfWorld>();
                
                newCellOfWorld.UpdateNeightbours();

                UpdateNeightbours();

                // UpdateNeightbours
                //foreach(KeyValuePair<Side, CellOfWorld> entry in newCellOfWorld.neightbours) {
                //    if (entry.Value != null && entry.Value != newCellOfWorld && entry.Value != this) {
                //        entry.Value.turnOnAllPlaceZones();
                //    }
                //}

                //foreach (KeyValuePair<Side, CellOfWorld> entry in neightbours) {
                //    if (entry.Value != null && entry.Value != newCellOfWorld && entry.Value != this) {
                //        entry.Value.turnOnAllPlaceZones();
                //    }
                //}

                return newCellOfWorld;
            }

            return null;
        }

        public void UpdateNeightbours()
        {
            for (int i = 0; i < Enum.GetValues(typeof(Side)).Length; i++) {
                if (neightbours.ContainsKey((Side)i)) 
                {
                    neightbours[(Side)i] = GetNeightbourInWorld((Side)i);
                } 
                else
                {
                    neightbours.Add((Side)i, GetNeightbourInWorld((Side)i));
                }
            }
        }


        public HexOfWorld GetNeightbourInWorld(Side side)
        {
            if (isPlaceFreeFromWorldCell(side) == false) {
                Collider[] colliders = Physics.OverlapSphere(spawnPoints[side].position, 0.25f, cellOfWorldMask);

                if(colliders.Length > 0) {
                    return colliders[0].GetComponent<HexOfWorld>();
                }
            }

            return null;
        }

        public int GetScoreBySideType(SideType sideType)
        {
            int finalScore = 0;

            foreach (KeyValuePair<Side, SideDataAndConditions> keyValue in data.sidesData) {
                if(keyValue.Value != null && keyValue.Value.sideData.type == sideType) {
                    finalScore += keyValue.Value.sideData.scores;
                }
            }

            return finalScore;
        }

        public void SetWeatherParticleEffect()
        {
            if (data.isHexWithWeatherParticleEffects)
            {
                string currentWeather = PlayerPrefs.GetString("CurrentWeather");

                switch (currentWeather)
                {
                    case "Winter":
                        break;
                    case "Autumn":
                        CreateParticleSystem(effectAutumn);
                        break;
                    case "Summer":
                        CreateParticleSystem(effectSummer);
                        break;
                }
            }
        }

        private void CreateParticleSystem(ParticleSystem _particleSystem)
        {
            if (!isTheWeatherHasBeenCreated)
            {
                ParticleSystem particle = Instantiate(_particleSystem, spawnPointWeatherConditions.transform.position, Quaternion.identity);
                particle.transform.SetParent(spawnPointWeatherConditions.transform);
                particle.gameObject.SetActive(true);
                particle.Play();

                isTheWeatherHasBeenCreated = true;
            }          
        }

        public enum Side
        {
            TopLeft = 0,
            TopRight = 1,
            Left = 2,
            Right = 3,
            BottomLeft = 4,
            BottomRight = 5
        }

        private void OnDrawGizmos()
        {
            foreach(KeyValuePair<Side, Transform> keyValue in spawnPoints){
                if (keyValue.Value != null) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(keyValue.Value.position, 0.1f);
                }
            }
        }
    }

    [System.Serializable]
    public class MaterialColor
    {
        public Material mat;
        public Color startColor;
    }
}