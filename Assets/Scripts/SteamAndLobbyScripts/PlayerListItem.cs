using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

/// <summary>
/// Represents a player list item in the lobby, displaying player information such as name, avatar, and ready status.
/// </summary>
public class PlayerListItem : MonoBehaviour
{
    public string PlayerName;
    public int ConnectionID;
    public ulong PlayerSteamID;
    private bool AvatarReceived;

    public TMP_Text playerNameText;
    public RawImage PlayerIcon;
    public TMP_Text playerReadyText;
    public bool isReady;

    protected Callback<AvatarImageLoaded_t> ImageLoaded;

    /// <summary>
    /// Initializes the player list item, setting up the callback for avatar image loading.
    /// </summary>
    private void Start()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    /// <summary>
    /// Changes the ready state of the player.
    /// </summary>
    public void ChangeReady()
    {
        if (isReady)
        {
            playerReadyText.color = Color.green;
        }
        else
        {
            playerReadyText.color = Color.red;
        }
    }

    /// <summary>
    /// Sets the player values for display in the UI, including name and avatar.
    /// </summary>
    public void SetPlayerValues()
    {
        playerNameText.text = PlayerName;
        ChangeReady();
        if (!AvatarReceived) { GetPlayerIcon(); }
    }

    /// <summary>
    /// Retrieves the player's avatar icon from Steam.
    /// </summary>
    void GetPlayerIcon()
    {
        int ImageId = SteamFriends.GetLargeFriendAvatar((CSteamID)PlayerSteamID);
        if (ImageId == -1) { return; }
        PlayerIcon.texture = GetSteamImageAsTexture(ImageId);
    }

    /// <summary>
    /// Callback method that is called when the avatar image is loaded from Steam.
    /// It updates the player's icon if the loaded image belongs to the player represented by this list item.
    /// </summary>
    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == PlayerSteamID)
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// Retrieves the Steam image as a Texture2D.
    /// </summary>
    /// <param name="iImage"></param>
    /// <returns name="texture"></returns>
    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        AvatarReceived = true;
        return texture;
    }
}
