using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField] private List<SlotImage> images;     //class to store total images
    [SerializeField] private List<SlotImage> Tempimages;     //class to store the result matrix
    [SerializeField] private List<SlotImage> ResultMatrix;     //class to store the result matrix

    [Header("Slots Transforms")]
    [SerializeField] private Transform[] Slot_Transform;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button TotalBetPlus_Button;
    [SerializeField] private Button TotalBetMinus_Button;
    [SerializeField] private Button LineBetPlus_Button;
    [SerializeField] private Button LineBetMinus_Button;
    [SerializeField] private Button MaxBet_Button;

    [Header("Animated Sprites")]
    [SerializeField] private Sprite[] BlueGem_Sprites;
    [SerializeField] private Sprite[] BlueGemEffect_Sprites;
    [SerializeField] private Sprite[] GreenGem_Sprites;
    [SerializeField] private Sprite[] GreenGemEffect_Sprites;
    [SerializeField] private Sprite[] PurpleGem_Sprites;
    [SerializeField] private Sprite[] PurpleGemEffect_Sprites;
    [SerializeField] private Sprite[] YellowGem_Sprites;
    [SerializeField] private Sprite[] YellowGemEffect_Sprites;
    [SerializeField] private Sprite[] OrangeGem_Sprites;
    [SerializeField] private Sprite[] OrangeGemEffect_Sprites;
    [SerializeField] private Sprite[] Seven_Sprites;
    [SerializeField] private Sprite[] Bar_Sprites;
    [SerializeField] private Sprite[] Rainbow_Sprites;
    [SerializeField] private Sprite[] comboSprites;
    [SerializeField] private Sprite[] BigWinAnimationSprites;
    [SerializeField] private Sprite[] HugeWinAnimationSprites;
    [SerializeField] private Sprite[] MegaWinAnimationSprites;


    [Header("Miscellaneous UI")]
    [SerializeField] private Sprite BigWin_Sprite;
    [SerializeField] private Sprite HugeWin_Sprite;
    [SerializeField] private Sprite MegaWin_Sprite;
    [SerializeField] private Sprite Empty_Sprite;
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private TMP_Text LineBet_text;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private Image[] Fill_Images;
    [SerializeField] private ImageAnimation[] RainbowAnimations; 

    [Header("Managers")]
    [SerializeField] private AudioController audioController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PayoutCalculation PayCalculator;
    [SerializeField] private SocketIOManager SocketManager;
    private Dictionary<int, Tweener> alltweens = new();
    private List<ImageAnimation> TempList=new();  //stores the sprites whose animation is running at present 
    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private bool IsAutoSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 10;
    private int numberOfSlots = 5;          //number of columns
    private ImageAnimation BaseImageAnimation;
    private bool isBaseAnimationRunning;
    private Coroutine BaseAnimationCoroutine;
    private Coroutine ComboAnimationCoroutine;
    private Coroutine PaylinesCoroutine;
    private int freeSpinIndex;
    private bool isStarBurst;
    private List<int> StarBurstColumns=new();

    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate
        {
            StartSlots();
        });

        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.RemoveAllListeners();
        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.AddListener(delegate
        {
            ChangeBet(true);
        });

        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.RemoveAllListeners();
        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.AddListener(delegate
        {
            ChangeBet(false);
        });

        if (LineBetPlus_Button) LineBetPlus_Button.onClick.RemoveAllListeners();
        if (LineBetPlus_Button) LineBetPlus_Button.onClick.AddListener(delegate
        {
            ChangeBet(true);
        });

        if (LineBetMinus_Button) LineBetMinus_Button.onClick.RemoveAllListeners();
        if (LineBetMinus_Button) LineBetMinus_Button.onClick.AddListener(delegate
        {
            ChangeBet(false);
        });

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(delegate
        {
            AutoSpin();
        });

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(delegate
        {
            StopAutoSpin();
        });

        if(MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if(MaxBet_Button) MaxBet_Button.onClick.AddListener(()=> {
            SetMaxBet();
        });
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {
            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        { 
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(.5f);
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        if (AutoSpinStop_Button) AutoSpinStop_Button.interactable = false;
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            AutoSpinStop_Button.interactable = true;
            tweenroutine = null;
            AutoSpinRoutine = null;
            IsAutoSpin = false;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (FreeSpinRoutine != null)
        {
            StopCoroutine(FreeSpinRoutine);
            FreeSpinRoutine = null;
        }
        FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = freeSpinIndex;
        while (i < spinchances)
        {
            i++;
            freeSpinIndex=i;
            StartSlots(false);
            yield return tweenroutine;
            yield return new WaitForSeconds(1.5f);
        }
        StarBurstColumns.Clear();
        isStarBurst=false;
        freeSpinIndex=0;
        ToggleButtonGrp(true);
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            SlotStart_Button.interactable = true;
        }
    }

    #region LinesCalculation

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> y_points = null;
        if(y_string.Count>0){
            y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
            PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
        }
    }
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }
    #endregion

    void SetMaxBet(){
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        SetFillImage();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.initialData.Bets.Count)
            {
                BetCounter = 0; // Loop back to the first bet
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.initialData.Bets.Count - 1; // Loop to the last bet
            }
        }
        SetFillImage();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void SetFillImage(){
        int fillBetCounter = BetCounter + 1;
        float fillAmount = Mathf.Clamp(1f/SocketManager.initialData.Bets.Count * fillBetCounter, 0, 1);
        // Debug.Log(fillAmount);
        foreach(Image i in Fill_Images){
            i.fillAmount= fillAmount;
        }
    } 

    #region InitialFunctions
    internal void ShuffleInitialMatrix()
    {
        for (int i = 0; i < images.Count; i++)
        {
            for (int j = 0; j < images[i].slotImages.Count; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length-1);
                images[i].slotImages[j].sprite = myImages[randomIndex];
                
                if (j >= 8 && j <= 10)
                {
                    var imageAnimation = images[i].slotImages[j].GetComponent<ImageAnimation>();
                    if (imageAnimation != null)
                    {
                        PopulateBaseAnimationSprites(imageAnimation, randomIndex);
                    }
                }
            }
        }

        StartCoroutine(AnimationLoop());
    }

    private IEnumerator AnimationLoop()
    {
        while (true)
        {
            if (!IsSpinning && !IsAutoSpin && !isStarBurst && !isBaseAnimationRunning)
            {
                if (BaseImageAnimation == null)
                {
                    int randomColumn = UnityEngine.Random.Range(0, 5);
                    int randomRow = UnityEngine.Random.Range(0, 3);

                    BaseImageAnimation = Tempimages[randomColumn].slotImages[randomRow].GetComponent<ImageAnimation>();
                    
                    if (BaseImageAnimation != null)
                    {
                        BaseAnimationCoroutine = StartCoroutine(BaseAnimation());
                    }
                }
            }
            else if(IsSpinning && IsAutoSpin && isStarBurst && isBaseAnimationRunning && BaseImageAnimation!=null){
                StopBaseAnimation();
            }

            yield return new WaitForSeconds(0.5f); // Add a delay to prevent overloading the game loop
        }
    }

    private IEnumerator BaseAnimation()
    {
        isBaseAnimationRunning = true;

        if (BaseImageAnimation != null)
        {
            if (BaseImageAnimation.textureArray.Count > 0)
            {
                BaseImageAnimation.StartAnimation();
                yield return new WaitUntil(() => BaseImageAnimation.textureArray[^1] == BaseImageAnimation.rendererDelegate.sprite);
                BaseImageAnimation.StopAnimation();
            }
            BaseImageAnimation = null;
        }
        isBaseAnimationRunning = false;
    }

    void StopBaseAnimation()
    {
        if (BaseImageAnimation != null)
        {
            StopCoroutine(BaseAnimationCoroutine);
            BaseImageAnimation.StopAnimation();
            BaseImageAnimation = null;
            isBaseAnimationRunning = false;
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        SetFillImage();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateBaseAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.id=val.ToString();
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                animScript.textureArray.AddRange(PurpleGem_Sprites);
                animScript.AnimationSpeed = PurpleGem_Sprites.Length-8;
                break;

            case 1:
                animScript.textureArray.AddRange(BlueGem_Sprites);
                animScript.AnimationSpeed = BlueGem_Sprites.Length-8;
                break;
            case 2:
                animScript.textureArray.AddRange(OrangeGem_Sprites);
                animScript.AnimationSpeed = OrangeGem_Sprites.Length-8;
                break;
            case 3:
                animScript.textureArray.AddRange(GreenGem_Sprites);
                animScript.AnimationSpeed = GreenGem_Sprites.Length-8;
                break;
            case 4:
                animScript.textureArray.AddRange(YellowGem_Sprites);
                animScript.AnimationSpeed = YellowGem_Sprites.Length-8;
                break;
            case 5:
                animScript.textureArray.AddRange(Seven_Sprites);
                animScript.AnimationSpeed = Seven_Sprites.Length;
                break;
            case 6:
                animScript.textureArray.AddRange(Bar_Sprites);
                animScript.AnimationSpeed = Bar_Sprites.Length;
                break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        //WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;

        StopBaseAnimation();
        StopGameAnimation();
        PayCalculator.ResetLines();
        uiManager.StopWinAnimation();

        tweenroutine = StartCoroutine(TweenRoutine());

        if (TotalWin_text) TotalWin_text.text = "0.00";
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine(bool bonus = false)
    {
        if (currentBalance < currentTotalBet && !isStarBurst) // Check if balance is sufficient to place the bet
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            yield break;
        }

        CheckSpinAudio = true;
        IsSpinning = true;
        ToggleButtonGrp(false);

        for (int i = 0; i < numberOfSlots; i++) // Initialize tweening for slot animations
        {
            if(!isStarBurst){
                InitializeTweening(Slot_Transform[i], i);
            }
            else{
                if(StarBurstColumns.Contains(i)){
                    continue;
                }
                else{
                    InitializeTweening(Slot_Transform[i], i);
                }
            }
        }

        if(!isStarBurst){
            BalanceDeduction(); //test
        }

        if(!isStarBurst){
            SocketManager.AccumulateResult(BetCounter);
            yield return new WaitUntil(() => SocketManager.isResultdone);
        }
        currentBalance = SocketManager.playerdata.Balance;
        StarBurstResponse starBurstResponse=null;
        if(SocketManager.resultData.isStarBurst){
            if(IsAutoSpin){
                IsAutoSpin=false;
                if(AutoSpinRoutine!=null){
                    StopCoroutine(AutoSpinRoutine);
                    AutoSpinRoutine=null;
                    AutoSpinStop_Button.gameObject.SetActive(false);
                    AutoSpin_Button.gameObject.SetActive(true);
                    AutoSpin_Button.interactable = false;
                }
            }
            if(!isStarBurst){
                isStarBurst=true;
                freeSpinIndex=0;    
            }
            starBurstResponse=SocketManager.resultData.starBurstResponse[freeSpinIndex];
        }
        else{
            isStarBurst=false;
            freeSpinIndex=0;
            StarBurstColumns.Clear();  
        }
        yield return new WaitForSeconds(1f);

        PopulateResultMatrix();

        for (int i = 0; i < numberOfSlots; i++) // Stop tweening for each slot
        {
            if(!isStarBurst){
                yield return StopTweening(Slot_Transform[i], i);
            }
            else{
                if(StarBurstColumns.Contains(i)){
                    continue;
                }
                else{
                    yield return StopTweening(Slot_Transform[i], i);
                }
            }
        }

        yield return new WaitForSeconds(1f);
        KillAllTweens();

        if(!isStarBurst){
            PaylinesCoroutine = StartCoroutine(CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit));
        }
        else{
            PaylinesCoroutine = StartCoroutine(CheckPayoutLineBackend(starBurstResponse.linesToEmit, starBurstResponse.FinalSymbolsToEmit, starBurstResponse));
        }
        yield return PaylinesCoroutine;

        if(isStarBurst && freeSpinIndex==0){
            IsSpinning=false;
            yield return new WaitForSeconds(2f);
            FreeSpin(SocketManager.resultData.starBurstResponse.Count-1);
            yield break;
        }

        if (!IsAutoSpin && !isStarBurst)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            IsSpinning = false;
            yield return new WaitForSeconds(2f);
        }
    }

    void PopulateResultMatrix(){
        if(!isStarBurst){
            for (int j = 0; j < SocketManager.resultData.ResultSymbols.Count; j++) // Update slot images based on the results
            {
                List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                for (int i = 0; i < 5; i++)
                {
                    if (ResultMatrix[j].slotImages[i]) ResultMatrix[j].slotImages[i].sprite = myImages[resultnum[i]];
                    string loc = i.ToString()+j.ToString();
                    if(resultnum[i] <= 4 && SocketManager.resultData.FinalsymbolsToEmit.Contains(loc)){
                        PopulateWinningsAnimationSprites(ResultMatrix[j].slotImages[i].transform.GetChild(0).GetComponent<ImageAnimation>(), resultnum[i]);
                    }
                    if(resultnum[i] <= 6){
                        PopulateBaseAnimationSprites(ResultMatrix[j].slotImages[i].GetComponent<ImageAnimation>(), resultnum[i]);
                    }
                }
            }
        }
        else{
            StarBurstResponse SBResponse=SocketManager.resultData.starBurstResponse[freeSpinIndex];
            List<List<int>> Result=SocketManager.resultData.starBurstResponse[freeSpinIndex].resultMatrix;
            for(int i=0;i<Tempimages.Count;i++){
                for (int j = 0; j < Tempimages[i].slotImages.Count; j++)
                {
                    int resultNum=Result[j][i];
                    if(Tempimages[i].slotImages[j] && !StarBurstColumns.Contains(i)){
                        Tempimages[i].slotImages[j].sprite=myImages[resultNum];
                        string loc = i.ToString()+j.ToString();
                        if(resultNum<=4 && SBResponse.FinalSymbolsToEmit.Contains(loc)){
                            PopulateWinningsAnimationSprites(Tempimages[i].slotImages[j].transform.GetChild(0).GetComponent<ImageAnimation>(), resultNum);
                        }
                        if(resultNum == 7){
                            PopulateWinningsAnimationSprites(Tempimages[i].slotImages[j].transform.GetChild(0).GetComponent<ImageAnimation>(), resultNum);
                        }
                        if(resultNum <= 6){
                            PopulateBaseAnimationSprites(Tempimages[i].slotImages[j].GetComponent<ImageAnimation>(), resultNum);
                        }
                    }
                }
            }
        }
    }

    private void PopulateWinningsAnimationSprites(ImageAnimation imageAnimation, int v)
    {
        imageAnimation.id=v.ToString();
        imageAnimation.textureArray.Clear();
        imageAnimation.textureArray.TrimExcess();
        imageAnimation.doLoopAnimation = false;
        switch(v){
            case 7:
                imageAnimation.textureArray.AddRange(Rainbow_Sprites);
                imageAnimation.AnimationSpeed=Rainbow_Sprites.Length;
                break;
            case 4:
                imageAnimation.textureArray.AddRange(YellowGemEffect_Sprites);
                imageAnimation.AnimationSpeed=YellowGemEffect_Sprites.Length;
                break;
            case 3:
                imageAnimation.textureArray.AddRange(GreenGemEffect_Sprites);
                imageAnimation.AnimationSpeed=GreenGemEffect_Sprites.Length;
                break;
            case 2:
                imageAnimation.textureArray.AddRange(OrangeGemEffect_Sprites);
                imageAnimation.AnimationSpeed=OrangeGemEffect_Sprites.Length;
                break;
            case 1:
                imageAnimation.textureArray.AddRange(BlueGemEffect_Sprites);
                imageAnimation.AnimationSpeed=BlueGemEffect_Sprites.Length;
                break;
            case 0:
                imageAnimation.textureArray.AddRange(PurpleGemEffect_Sprites);
                imageAnimation.AnimationSpeed=PurpleGemEffect_Sprites.Length;
                break;
        }
    }
    #endregion

    internal bool CheckWinPopups(double amount)
    {
        if (amount >= currentTotalBet * 5 && amount < currentTotalBet * 10)
        {
            return true;
        }
        else if (amount >= currentTotalBet * 10 && amount < currentTotalBet * 15)
        {
            return true;
        }
        else if (amount >= currentTotalBet * 15)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void WinningsTextAnimation(double amount)
    {
        float time=0.8f;
        double winnings = 0;
        if(!double.TryParse(Balance_text.text, out double balance)){
            Debug.Log("Error while conversion");
        }
        double newBalance = balance + amount;
        DOTween.To(() => winnings, val => winnings = val, amount, time).OnUpdate(() =>
        {
            if (TotalWin_text)
            {
                TotalWin_text.text = winnings.ToString("f2");
            }
        });
        DOTween.To(() => balance, (val) => balance = val, newBalance, time).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = balance.ToString("f2");
        });
    }

    private void BalanceDeduction()
    {
        if(!double.TryParse(TotalBet_text.text, out double bet)){
            Debug.Log("Error while conversion");
        }
        if(!double.TryParse(Balance_text.text, out double balance)){
            Debug.Log("Error while conversion");
        }
        double initAmount = balance;
        balance -= bet;
        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });
    }

    //generate the payout lines generated 
    private IEnumerator CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, StarBurstResponse SBresponse = null)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {
            if(!isStarBurst){
                bool allIdsAreSame = true;
                string firstId = Tempimages[2].slotImages[0].GetComponent<ImageAnimation>().id;

                for (int i = 1; i < Tempimages[2].slotImages.Count; i++)
                {
                    string currentId = Tempimages[2].slotImages[i].GetComponent<ImageAnimation>().id;
                    if (currentId != firstId)
                    {
                        allIdsAreSame = false;
                        break; // Exit the loop early if a mismatch is found
                    }
                }
                if(allIdsAreSame){
                    for(int i=Tempimages[2].slotImages.Count-1;i>=0;i--){
                        Tempimages[2].slotImages[i].transform.GetChild(1).GetComponent<ImageAnimation>().StartAnimation();
                        yield return new WaitForSeconds(1f);
                    }
                }
            }
            if(isStarBurst && StarBurstColumns.Count<4){
                // Debug.Log("Here");
                List<int> currentAnimationColumns=new();
                for(int i=1;i<Tempimages.Count-1;i++){
                    for(int j=0;j<Tempimages[i].slotImages.Count;j++){
                        int resultNum=SBresponse.resultMatrix[j][i];
                        if(resultNum==7 && !StarBurstColumns.Contains(i)){
                            StarBurstColumns.Add(i);
                            currentAnimationColumns.Add(i);
                            for(int k=0;k<Tempimages[i].slotImages.Count;k++){
                                PopulateWinningsAnimationSprites(Tempimages[i].slotImages[k].transform.GetChild(0).GetComponent<ImageAnimation>(), 7);
                            }
                            break;
                        }
                    }
                }
                foreach(int i in currentAnimationColumns){
                    audioController.PlayWLAudio("Star");
                    RainbowAnimations[i].StartAnimation();
                    RainbowAnimations[i].rendererDelegate.DOFade(1, 1f);
                    for(int j=0;j<Tempimages[i].slotImages.Count;j++){
                        ImageAnimation RainbowRotationAnimation = Tempimages[i].slotImages[j].transform.GetChild(0).GetComponent<ImageAnimation>();
                        Tempimages[i].slotImages[j].DOFade(0, 0.2f);
                        StartCoroutine(RainbowRotationCoroutine(RainbowRotationAnimation, Tempimages[i].slotImages[j]));
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                yield return new WaitForSeconds(2f);
            }

            FadeOutImages();
        
            List<Transform> transforms = new();
            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    // Debug.Log(Tempimages[points_anim[k] / 10 % 10].slotImages[points_anim[k] % 10].name);
                    if (points_anim[k] >= 10)
                    {
                        Tempimages[points_anim[k] / 10 % 10].slotImages[points_anim[k] % 10].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        transforms.Add(Tempimages[points_anim[k] / 10 % 10].slotImages[points_anim[k] % 10].transform.GetChild(0));
                    }
                    else
                    {
                        Tempimages[0].slotImages[points_anim[k]].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        transforms.Add(Tempimages[0].slotImages[points_anim[k]].transform.GetChild(0));
                    }
                }
            }

            bool CanPlayComboAnim;
            if(isStarBurst){
                CanPlayComboAnim = !CheckWinPopups(SBresponse.payout);
            }
            else{
                CanPlayComboAnim = !CheckWinPopups(SocketManager.playerdata.currentWining);
            }

            
            if(!isStarBurst){
                if (SocketManager.playerdata.currentWining>0){
                    WinningsTextAnimation(SocketManager.playerdata.currentWining);
                }
            }
            else{
                if (SBresponse.payout>0){
                    WinningsTextAnimation(SBresponse.payout);
                }
            }

            if(!CanPlayComboAnim){
                StartCoroutine(uiManager.BigWinStartAnim());
                uiManager.BigWinAnimating=true;
                if(isStarBurst){
                    TriggerHugeWinAnimation(SBresponse.payout);
                }
                else{
                    TriggerHugeWinAnimation(SocketManager.playerdata.currentWining);
                }
            }
            else{
                audioController.PlayWLAudio("win");
            }

            for (int i = 0; i < transforms.Count; i++)
            {
                StartGameAnimation(transforms[i]);
                yield return new WaitForSeconds(0.2f);

                if(CanPlayComboAnim){
                    if ((i + 1) % 3 == 0)
                    {
                        Sprite comboSprite = null;

                        if ((i + 1) / 3 == 1 && LineId.Count >= 2)
                        {
                            comboSprite = comboSprites[0];
                        }
                        else if ((i + 1) / 3 == 2 && LineId.Count >= 3)
                        {
                            comboSprite = comboSprites[1];
                        }
                        else if ((i + 1) / 3 == 3 && LineId.Count >= 4)
                        {
                            comboSprite = comboSprites[2];
                        }
                        if (comboSprite != null)
                        {
                            while (uiManager.isComboSpritesAnimating)
                            {
                                yield return null;
                            }
                            ComboAnimationCoroutine = StartCoroutine(uiManager.AnimateSprite(comboSprite));
                        }
                    }
                } 
            }

            yield return new WaitUntil(()=> !uiManager.BigWinAnimating);

            if(TempList.Count>0){
                if(TempList[^1].textureArray.Count==0){
                    for(int j=TempList.Count-1;j>=0;j--){
                        if(TempList[j].textureArray.Count>0){
                            yield return new WaitUntil(()=> TempList[j].textureArray[^1]==TempList[j].rendererDelegate.sprite);
                            break;
                        }
                    }
                }
                else{
                    yield return new WaitUntil(()=> TempList[^1].textureArray[^1]==TempList[^1].rendererDelegate.sprite);
                }
            }

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }
        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
        }

        CheckSpinAudio = false;
    }

    private void TriggerHugeWinAnimation(double amount){
        if (amount >= currentTotalBet * 5 && amount < currentTotalBet * 10)
        {
            audioController.PlayWLAudio("megaWin");
            StartCoroutine(uiManager.StartWinAnimation(BigWin_Sprite, BigWinAnimationSprites));
        }
        else if (amount >= currentTotalBet * 10 && amount < currentTotalBet * 15)
        {
            audioController.PlayWLAudio("bigwin");
            StartCoroutine(uiManager.StartWinAnimation(HugeWin_Sprite, HugeWinAnimationSprites));
        }
        else if (amount >= currentTotalBet * 15)
        {
            audioController.PlayWLAudio("bigwin");
            StartCoroutine(uiManager.StartWinAnimation(MegaWin_Sprite, MegaWinAnimationSprites));
        }
    }

    private IEnumerator RainbowRotationCoroutine(ImageAnimation RainbowRotationAnimation, Image image){
        RainbowRotationAnimation.StartAnimation();
        yield return new WaitUntil(()=> RainbowRotationAnimation.textureArray[^10]==RainbowRotationAnimation.rendererDelegate.sprite);
        image.sprite=myImages[7];
        yield return new WaitUntil(()=> RainbowRotationAnimation.textureArray[^1]==RainbowRotationAnimation.rendererDelegate.sprite);
        RainbowRotationAnimation.StopAnimation();
        RainbowRotationAnimation.rendererDelegate.sprite=Empty_Sprite;
        image.color=new Color(1,1,1,1);
        RainbowRotationAnimation.textureArray.Clear();
        RainbowRotationAnimation.textureArray.TrimExcess();
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button && !IsAutoSpin) AutoSpin_Button.interactable = toggle;
        if (LineBetMinus_Button) LineBetMinus_Button.interactable = toggle;
        if (TotalBetMinus_Button) TotalBetMinus_Button.interactable = toggle;
        if (LineBetPlus_Button) LineBetPlus_Button.interactable = toggle;
        if (TotalBetPlus_Button) TotalBetPlus_Button.interactable = toggle;
    }

    private void StartGameAnimation(Transform animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        if (temp.textureArray.Count > 0)
        {
            temp.StartAnimation();
            TempList.Add(temp);
            temp.IsAnim = true;
        }
    }

    private void FadeOutImages()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < Tempimages[i].slotImages.Count; j++)
            {
                Tempimages[i].slotImages[j].GetComponent<Image>().color = new Color(0.33f, 0.33f, 0.33f, 1);
            }
        }
    }

    internal void StopGameAnimation()
    {
        if(PaylinesCoroutine!=null){
            StopCoroutine(PaylinesCoroutine);
        }
        for(int i=0;i<ResultMatrix.Count;i++){
            for(int j=0;j<ResultMatrix[i].slotImages.Count;j++){
                ImageAnimation BaseAnim = ResultMatrix[i].slotImages[j].GetComponent<ImageAnimation>();
                ImageAnimation WinningsAnim = ResultMatrix[i].slotImages[j].transform.GetChild(0).GetComponent<ImageAnimation>();
                if(BaseAnim.textureArray.Count>0){
                    BaseAnim.StopAnimation();
                }
                BaseAnim.textureArray.Clear();
                BaseAnim.textureArray.TrimExcess();
                if(WinningsAnim.textureArray.Count>0){
                    WinningsAnim.StopAnimation();
                }
                WinningsAnim.textureArray.Clear();
                WinningsAnim.textureArray.TrimExcess();
                ResultMatrix[i].slotImages[j].color=new Color(1, 1, 1, 1);
            }
        }

        for(int i=0;i<Tempimages[2].slotImages.Count;i++){
            if(Tempimages[2].slotImages[i].transform.GetChild(1).GetComponent<ImageAnimation>().currentAnimationState==ImageAnimation.ImageState.PLAYING){
                Tempimages[2].slotImages[i].transform.GetChild(1).GetComponent<ImageAnimation>().StopAnimation();
            }
        }

        if(uiManager.isComboSpritesAnimating){
            StopCoroutine(ComboAnimationCoroutine);
            uiManager.isComboSpritesAnimating = false;
        }

        if(!isStarBurst){
            foreach(ImageAnimation i in RainbowAnimations){
                if(i!=null && i.currentAnimationState == ImageAnimation.ImageState.PLAYING){
                    i.rendererDelegate.DOFade(0, 0.5f).OnComplete(()=>{
                        i.StopAnimation();
                    });
                }
            }
        }

        PayCalculator.ResetStaticLine();
        PayCalculator.ResetLines();
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform, int index)
    {
        Tweener tweener = null;
        slotTransform.DOLocalMoveY(-3221f, .7f)
        .SetEase(Ease.InBack)
        .OnComplete(()=> {
            slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, -959f);

            tweener = slotTransform.DOLocalMoveY(-3221f, .7f)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
            alltweens.Add(index, tweener);
        });
    }

    private IEnumerator StopTweening(Transform slotTransform, int index)
    {
        bool IsRegister = false;
        yield return alltweens[index].OnStepComplete(delegate { IsRegister = true; });
        yield return new WaitUntil(() => IsRegister);

        alltweens[index].Kill();
        slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, -1372f);
        alltweens[index] = slotTransform.DOLocalMoveY(-1691f+323.195f, .7f).SetEase(Ease.OutQuint).OnComplete( ()=> {alltweens[index].Kill();} );
        if (audioController) audioController.PlayWLAudio("spinStop");
    }


    private void KillAllTweens()
    {
        foreach(var item in alltweens){
            item.Value.Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}