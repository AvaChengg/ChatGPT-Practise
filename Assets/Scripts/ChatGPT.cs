using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatGPT : MonoBehaviour
{
    [Header("UI Settings:")]
    [SerializeField] private InputField _inputField;
    [SerializeField] private Button _button;
    [SerializeField] private ScrollRect _scrollRect;

    [Header("ChatGPT Setting:")]
    [SerializeField] private RectTransform _sent;
    [SerializeField] private RectTransform _received;

    // set scroll size
    private float _height;
    private string voiceText;

    // get data
    private TextToSpeech _textToSpeech;
    private OpenAIApi _openAI = new OpenAIApi();
    private List<ChatMessage> _messages = new List<ChatMessage>();
    private string _prompt = "Act as a random stranger in a chat room and reply to the questions. Don't break character. Don't ever mention that you are an AI model.";

    private void Start()
    {
        _button.onClick.AddListener(SendReply);
        _textToSpeech = GetComponent<TextToSpeech>();
    }

    private void AppendMessage(ChatMessage message)
    {
        _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

        var item = Instantiate(message.Role == "user" ? _sent : _received, _scrollRect.content);
        item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
        if (message.Role != "user") voiceText = item.GetChild(0).GetChild(0).GetComponent<Text>().text;
        item.anchoredPosition = new Vector2(0, -_height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        _height += item.sizeDelta.y;
        _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _height);
        _scrollRect.verticalNormalizedPosition = 0;
    }

    private async void SendReply()
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = _inputField.text
        };

        AppendMessage(newMessage);

        if (_messages.Count == 0) newMessage.Content = _prompt + "\n" + _inputField.text;

        _messages.Add(newMessage);

        _button.enabled = false;
        _inputField.text = "";
        _inputField.enabled = false;

        // Complete the instruction
        var completionResponse = await _openAI.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo-0301",
            Messages = _messages
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();

            _messages.Add(message);
            AppendMessage(message);
            _textToSpeech.PlayAiSpeech(voiceText);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }

        _button.enabled = true;
        _inputField.enabled = true;
    }
}
