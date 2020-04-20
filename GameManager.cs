using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Interfaz")]
    public GameObject PlayerInterface;
    public GameObject gameOverDisplay;
    public Text gameOverSentence;
    public Text ScoreDisplay;

    public GameObject titulo;
    public GameObject tutorial;

    [Header("Parámetros Magos")]    
    public Slider mageSlider;
    public Animator mageAnimator;
    public ParticleSystem fireSpellParcicles;
    public ParticleSystem iceSpellParcicles;
    public float particleEmisionMultiplier = 1;
    public float particleLifetimeMultiplier = 1;
    public float particleSpeedMultipier = 1;

    [Header("Parámetros Climas")]
    public Animator climeAnim;
    public Clime[] climes;
    private Clime actualClime;
    private float actualTempVar;    


    [Header("Parámetros Huevo")]
    public float eggTemperatureLimit;
    float eggTemp = 0;
    public Animator eggStateAnim;
    public GameObject egg;
    public ParticleSystem eggExplosion;
    public AudioSource eggSound;

    [Header("Parámetros de Juego")]

    bool playing;
    public float minClimeTime = 2f;
    float OldMinClimeTime;
    public float maxClimeTime = 5f;
    float OldMaxClimeTime;
    public float dificultyMultiplier = 0.91f;
    float score;
    
    bool climeChangeAviable;
    int winCount;

    [Header("Frases de Game Over")]
    public GameOverPhrase[] gameOverPhrases;
    

    // Start is called before the first frame update
    void Start()
    {
        playing = false;
        PlayerInterface.SetActive(false);
        gameOverDisplay.SetActive(false);
        titulo.SetActive(true);
        tutorial.SetActive(false);
        OldMinClimeTime = minClimeTime;
        OldMaxClimeTime = maxClimeTime;
        //StartCoroutine(FirstCorroutine());
        //InvokeRepeating("SetClime", 3.4f, Random.Range(1f, 3.5f));
    }

    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            score += Time.deltaTime;
        }

        if (winCount > 4)
        {
            winCount = 0;
            minClimeTime *= dificultyMultiplier;
            maxClimeTime *= dificultyMultiplier;
        }
        
        if (climeChangeAviable)
        {
            StartCoroutine(ClimatechangeCorroutine());
        }

        if (Mathf.Abs(eggTemp)> eggTemperatureLimit  && playing)
        {
            //StopAllCoroutines();
            StartCoroutine(CorrutinaMuerte());
        }

        mageAnimator.SetFloat("Power", mageSlider.value);
        SetMagicSpells(mageSlider.value);
        UpdateEggState();
    }

    void SetMagicSpells(float value)
    {
        string magic;

        if (value < -10)
        {
            magic = "ice"; 
            var emission = iceSpellParcicles.emission;
            var main = iceSpellParcicles.main;
            emission.rateOverTime = value * particleEmisionMultiplier * (-1);
            main.startLifetime = value * particleLifetimeMultiplier * (-1);
            main.startSpeed = value * particleSpeedMultipier * (-1);            
        }

        else if (value > 10)
        {
            magic = "fire";
            var emission = fireSpellParcicles.emission;
            var main = fireSpellParcicles.main;
            emission.rateOverTime = value * particleEmisionMultiplier;
            main.startLifetime = value * particleLifetimeMultiplier;
            main.startSpeed = value * particleSpeedMultipier;
        }

        else
        {
            magic = "none";
        }

        switch (magic)
        {
            case "ice":
                var fireEmission = fireSpellParcicles.emission;
                var fireMain = fireSpellParcicles.main;
                fireEmission.rateOverTime = 0;
                fireMain.startLifetime = 0;
                fireMain.startSpeed = 0;

                break;
            case "fire":
                var iceEmission = iceSpellParcicles.emission;
                var iceMain = iceSpellParcicles.main;
                iceEmission.rateOverTime = 0;
                iceMain.startLifetime = 0;
                iceMain.startSpeed = 0;
                break;
            case "none":
                var FireEmission = fireSpellParcicles.emission;
                var FireMain = fireSpellParcicles.main;
                FireEmission.rateOverTime = 0;
                FireMain.startLifetime = 0;
                FireMain.startSpeed = 0;
                var IceEmission = iceSpellParcicles.emission;
                var IceMain = iceSpellParcicles.main;
                IceEmission.rateOverTime = 0;
                IceMain.startLifetime = 0;
                IceMain.startSpeed = 0;
                break;
        }
    }

    void SetClime()
    {        
        actualClime.particleSystem.SetActive(false);
        int index = Random.Range(0, climes.Length);
        actualClime = climes[index];
        actualClime.particleSystem.SetActive(true);
        actualTempVar = Random.Range(actualClime.minTempChange, actualClime.maxTempChange);
        climeAnim.SetTrigger(actualClime.name);
    }
    void SetFirstClime()
    {
        if (actualClime != null)
        {
            actualClime.particleSystem.SetActive(false);
        }        
        //int index = Random.Range(0, climes.Length);
        actualClime = climes[0];
        actualClime.particleSystem.SetActive(true);
        actualTempVar = Random.Range(actualClime.minTempChange, actualClime.maxTempChange);
        climeAnim.SetTrigger(actualClime.name);
    }

    void UpdateEggState()
    {
        if (playing)
        {
            eggTemp += actualTempVar * Time.deltaTime;
            if (Mathf.Abs(mageSlider.value) > 10)
            {
                eggTemp += mageSlider.value * Time.deltaTime;
            }
            Mathf.Clamp(eggTemp, -eggTemperatureLimit, eggTemperatureLimit);
            eggStateAnim.SetFloat("eggTemp", eggTemp);
        }        
    }

    IEnumerator FirstCorroutine()
    {
        score = 0f;
        egg.SetActive(true);
        playing = true;
        gameOverDisplay.SetActive(false);
        climeChangeAviable = false;
        PlayerInterface.SetActive(true);
        mageSlider.value = 0;
        SetFirstClime();
        eggTemp = 0;
        minClimeTime = OldMinClimeTime;
        maxClimeTime = OldMaxClimeTime;
        yield return new WaitForSeconds(3f);
        climeChangeAviable = true;
    }

    IEnumerator ClimatechangeCorroutine()
    {
        climeChangeAviable = false;
        SetClime();
        yield return new WaitForSeconds(Random.Range(minClimeTime, maxClimeTime));
        winCount++;
        climeChangeAviable = true;
    }

    IEnumerator CorrutinaMuerte()
    {  
        eggSound.Play();
        egg.SetActive(false);
        eggExplosion.Emit(30);
        playing = false;
        int finalScore = (int)(score % 60);
        mageSlider.value = 0;
        climeChangeAviable = false;
        PlayerInterface.SetActive(false);        
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Game Over");
        int i = Random.Range(0, gameOverPhrases.Length);
        ScoreDisplay.text = "your score: " + finalScore.ToString() + " sec";
        gameOverSentence.text = gameOverPhrases[i].sentence;
        gameOverDisplay.SetActive(true);
        StopAllCoroutines();
    }

    public void Retry()
    {
        //SceneManager.LoadScene("Game");
        StartCoroutine(FirstCorroutine());
    }

    public void PlayGame()
    {
        titulo.SetActive(false);
        tutorial.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void EndTutorial()
    {
        tutorial.SetActive(false);
        StartCoroutine(FirstCorroutine());
    }



    

}
