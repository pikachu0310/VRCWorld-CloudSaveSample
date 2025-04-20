using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using TMPro;
using VRC.SDK3.Components;
using VRC.SDK3.Data;

public class Test : UdonSharpBehaviour
{
    private const string ApiBaseUrl = "https://push.trap.games/api";
    private const string GlobalSecret = "secret-medal-pusher";

    //TODO ここら辺のデータは実際に保存するデータがある場所のを使うようにする。
    [UdonSynced] public int version = 1;
    [UdonSynced] public int have_medal;
    [UdonSynced] public int in_medal;
    [UdonSynced] public int out_medal;
    [UdonSynced] public int slot_hit;
    [UdonSynced] public int get_shirbe;
    [UdonSynced] public int start_slot;
    [UdonSynced] public int shirbe_buy300;
    [UdonSynced] public int medal_1;
    [UdonSynced] public int medal_2;
    [UdonSynced] public int medal_3;
    [UdonSynced] public int medal_4;
    [UdonSynced] public int medal_5;
    [UdonSynced] public int R_medal;
    [UdonSynced] public int total_play_time;
    [UdonSynced] public int fever;

    [SerializeField] private VRCUrlInputField urlInputField;
    [SerializeField] private TMP_InputField urlSaveCopyText;
    [SerializeField] private TMP_InputField urlLoadCopyText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private UdonHashLib hashLibrary;

    private int _sendURLCount = 0;

    private void Start()
    {
        resultText.text = "";
        SetLoadURL(Networking.LocalPlayer.displayName);
    }

    private void Update()
    {
        UpdateSaveURL(); //TODO これをセーブするタイミングで呼び出すようにする
    }

    public void SendGetRequestToServer()
    {
        _sendURLCount++;
        VRCStringDownloader.LoadUrl(urlInputField.GetUrl(), (IUdonEventReceiver)this);
    }

    public void UpdateSaveURL() //TODO 書き換えが必要
    {
        UpdateSaveURLByUserData(
            Networking.LocalPlayer.displayName, version,
            have_medal, in_medal, out_medal, slot_hit, get_shirbe,
            start_slot, shirbe_buy300, medal_1, medal_2, medal_3,
            medal_4, medal_5, R_medal, total_play_time, fever
        );
    }

    public void UpdateSaveURLByUserData(
        string playerId, int version,
        int have_medal, int in_medal, int out_medal, int slot_hit, int get_shirbe,
        int start_slot, int shirbe_buy300, int medal_1, int medal_2, int medal_3,
        int medal_4, int medal_5, int R_medal, int total_play_time, int fever
    )
    {
        string[] parameters = new string[]
        {
            $"version={version}",
            $"user_id={playerId}",
            $"have_medal={have_medal}",
            $"in_medal={in_medal}",
            $"out_medal={out_medal}",
            $"slot_hit={slot_hit}",
            $"get_shirbe={get_shirbe}",
            $"start_slot={start_slot}",
            $"shirbe_buy300={shirbe_buy300}",
            $"medal_1={medal_1}",
            $"medal_2={medal_2}",
            $"medal_3={medal_3}",
            $"medal_4={medal_4}",
            $"medal_5={medal_5}",
            $"R_medal={R_medal}",
            $"total_play_time={total_play_time}",
            $"fever={fever}"
        };

        string queryStr = SortStringArray(parameters);
        string signature = GenerateUserSignature(playerId, queryStr);
        string fullUrl = $"{ApiBaseUrl}/data?{queryStr}&sig={signature}";
        urlSaveCopyText.text = fullUrl;
    }

    // ユーザーデータ取得のURLを更新
    public void SetLoadURL(string playerId)
    {
        string urlStr = $"{ApiBaseUrl}/users/{playerId}/data";
        urlLoadCopyText.text = urlStr;
    }

    // ランキング取得のURLを更新
    public void FetchRankings()
    {
        string urlStr = $"{ApiBaseUrl}/rankings?sort=have_medal&limit=50";
        urlLoadCopyText.text = urlStr;
    }

    private string SortStringArray(string[] array)
    {
        Array.Sort((Array)array);
        return string.Join("&", array);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        resultText.text = $"Failed {_sendURLCount} : {result.Result}";
        Debug.Log($"Data loaded successfully: {result.Result}");
        if (result.Result.Length == 10)
        {
            resultText.text = $"Save Success! {_sendURLCount}";
            return;
        }

        if (VRCJson.TryDeserializeFromJson(result.Result, out DataToken dataToken))
        {
            if (dataToken.TokenType == TokenType.DataDictionary)
            {
                Debug.Log(
                    $"Successfully deserialized as a dictionary with {dataToken.DataDictionary.Count} items.");
                UpdateData(dataToken);
            }
            else if (dataToken.TokenType == TokenType.DataList)
            {
                Debug.Log($"Successfully deserialized as a list with {dataToken.DataList.Count} items.");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to Deserialize json {result.Result} - {dataToken.ToString()}");
            return;
        }
    }

    private void UpdateData(DataToken dataToken)  //TODO この関数はデータが保存されてる場所に移動させて、publicからこのスクリプトから呼び出すようにする。
    {
        var username = GetStringValue(dataToken, "user_id");
        if (username != Networking.LocalPlayer.displayName)
        {
            resultText.text = $"Not your data! {_sendURLCount}";
            return;
        }

        have_medal = GetIntValue(dataToken, "have_medal");
        in_medal = GetIntValue(dataToken, "in_medal");
        out_medal = GetIntValue(dataToken, "out_medal");
        slot_hit = GetIntValue(dataToken, "slot_hit");
        get_shirbe = GetIntValue(dataToken, "get_shirbe");
        start_slot = GetIntValue(dataToken, "start_slot");
        shirbe_buy300 = GetIntValue(dataToken, "shirbe_buy300");
        medal_1 = GetIntValue(dataToken, "medal_1");
        medal_2 = GetIntValue(dataToken, "medal_2");
        medal_3 = GetIntValue(dataToken, "medal_3");
        medal_4 = GetIntValue(dataToken, "medal_4");
        medal_5 = GetIntValue(dataToken, "medal_5");
        R_medal = GetIntValue(dataToken, "R_medal");
        total_play_time = GetIntValue(dataToken, "total_play_time");
        fever = GetIntValue(dataToken, "fever");
        resultText.text = $"Load Success! {_sendURLCount}";
    }

    private int GetIntValue(DataToken dataToken, string key)
    {
        if (dataToken.DataDictionary.TryGetValue(key, out DataToken value))
        {
            return int.Parse(value.ToString());
        }

        return 0;
    }

    private string GetStringValue(DataToken dataToken, string key)
    {
        if (dataToken.DataDictionary.TryGetValue(key, out DataToken value))
        {
            return value.ToString();
        }

        return string.Empty;
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogWarning($"Error loading data: {result.ErrorCode} - {result.Error}");
        if (result.ErrorCode == 404)
        {
            resultText.text = $"Not Found! {_sendURLCount}";
            return;
        }
        {
            resultText.text = $"Failed...{_sendURLCount} {result.ErrorCode} - {result.Error}";
        }
    }

    /*
     * ここからHMAC-SHA256の実装
     */
    #region HMAC-SHA256の実装 (暗号化)

    private byte[] ToUtf8(string str)
    {
        // UTF8に変換（簡易実装: ASCII相当の範囲のみ）
        byte[] bytes = new byte[str.Length];
        for (int i = 0; i < str.Length; i++)
        {
            bytes[i] = (byte)str[i];
        }

        return bytes;
    }

    // 署名生成用の関数（追加）
    private string GenerateUserSignature(string userId, string queryString)
    {
        // STEP 1: ユーザー固有のシークレットを生成 (GlobalSecretをキーとしてuserIdを署名)
        // STEP 2: パラメータ文字列(queryString)は既にソート済みで生成済みである前提
        // STEP 3: 生成したユーザー固有の秘密鍵(userSecret)をキーとしてqueryStringを署名
        string userSecretHex = HmacSha256(GlobalSecret, userId);
        byte[] userSecret = HexToBytes(userSecretHex);
        return HmacSha256_Bytes(userSecret, queryString);
    }

    // byte[]型のキーを受け取るバージョンのHmacSha256 (追加)
    public string HmacSha256_Bytes(byte[] key, string message)
    {
        byte[] msg = ToUtf8(message);
        const int blockSize = 64;

        if (key.Length > blockSize)
        {
            string hashed = hashLibrary.SHA256_Bytes(key);
            key = HexToBytes(hashed);
        }

        byte[] k = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
        {
            k[i] = i < key.Length ? key[i] : (byte)0x00;
        }

        byte[] ipad = new byte[blockSize];
        byte[] opad = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
        {
            ipad[i] = (byte)(k[i] ^ 0x36);
            opad[i] = (byte)(k[i] ^ 0x5c);
        }

        byte[] innerInput = Concat(ipad, msg);
        string innerHashHex = hashLibrary.SHA256_Bytes(innerInput);
        byte[] innerHash = HexToBytes(innerHashHex);

        byte[] outerInput = Concat(opad, innerHash);
        return hashLibrary.SHA256_Bytes(outerInput);
    }


    public string HmacSha256(string keyStr, string message)
    {
        byte[] key = ToUtf8(keyStr);
        byte[] msg = ToUtf8(message);
        const int blockSize = 64;

        // 1. キーを整える
        if (key.Length > blockSize)
        {
            string hashed = hashLibrary.SHA256_Bytes(key);
            key = HexToBytes(hashed);
        }

        byte[] k = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
        {
            k[i] = i < key.Length ? key[i] : (byte)0x00;
        }

        // 2. ipad, opad と XOR を生成
        byte[] ipad = new byte[blockSize];
        byte[] opad = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
        {
            ipad[i] = (byte)(k[i] ^ 0x36);
            opad[i] = (byte)(k[i] ^ 0x5c);
        }

        // 3. inner hash = SHA256(ipad || message)
        byte[] innerInput = Concat(ipad, msg);
        string innerHashHex = hashLibrary.SHA256_Bytes(innerInput);
        byte[] innerHash = HexToBytes(innerHashHex);

        // 4. outer hash = SHA256(opad || innerHash)
        byte[] outerInput = Concat(opad, innerHash);
        return hashLibrary.SHA256_Bytes(outerInput);
    }

    private byte[] Concat(byte[] a, byte[] b)
    {
        byte[] result = new byte[a.Length + b.Length];
        for (int i = 0; i < a.Length; i++) result[i] = a[i];
        for (int i = 0; i < b.Length; i++) result[a.Length + i] = b[i];
        return result;
    }

    private byte[] HexToBytes(string hex)
    {
        int len = hex.Length / 2;
        byte[] result = new byte[len];
        for (int i = 0; i < len; i++)
        {
            string byteStr = hex.Substring(i * 2, 2);
            result[i] = (byte)ConvertHex(byteStr);
        }

        return result;
    }

    private int ConvertHex(string hex)
    {
        int high = HexCharToInt(hex[0]);
        int low = HexCharToInt(hex[1]);
        return (high << 4) + low;
    }

    private int HexCharToInt(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        return 0;
    }

    #endregion
}
