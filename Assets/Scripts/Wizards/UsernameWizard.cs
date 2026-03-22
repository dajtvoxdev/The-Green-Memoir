using UnityEngine;
using UnityEngine.UI;

public class UsernameWizard : MonoBehaviour
{
    public Text username;
    public Text gold;
    public Text diamond;

    public GameObject usernameWizard;
    public InputField ipUsername;
    public Button buttonOK;

    private FirebaseDatabaseManager databaseManager;

    void Start()
    {
        databaseManager = GameObject.Find("DatabaseManager")?.GetComponent<FirebaseDatabaseManager>();

        if (LoadDataManager.userInGame != null)
        {
            InitializeFromUserData();
        }

        buttonOK.onClick.AddListener(SetNewUsername);
    }

    private void InitializeFromUserData()
    {
        if (string.IsNullOrEmpty(LoadDataManager.userInGame.Name))
        {
            usernameWizard.SetActive(true);
        }
        else
        {
            username.text = LoadDataManager.userInGame.Name;
        }

        gold.text = LocalizationManager.LocalizeText("Gold: " + LoadDataManager.userInGame.Gold.ToString());
        diamond.text = LocalizationManager.LocalizeText("Diamond: " + LoadDataManager.userInGame.Diamond.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNewUsername()
    {
        if(ipUsername.text != "")
        {
            LoadDataManager.userInGame.Name = ipUsername.text;
            if (LoadDataManager.Instance != null)
            {
                LoadDataManager.Instance.SaveUserInGame();
            }
            else if (databaseManager != null)
            {
                databaseManager.WriteDatabase(FirebaseUserPaths.GetUserProfilePath(LoadDataManager.firebaseUser.UserId), LoadDataManager.userInGame.ToString());
            }

            username.text = ipUsername.text;
            usernameWizard.SetActive(false);
        }
    }
}
