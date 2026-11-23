using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Editor
{
    public class DialogueEditorWindow : EditorWindow
    {
        private const string DIALOGUES_PATH = "Assets/Resources/Dialogues.json";
        private const string RESPONSES_PATH = "Assets/Resources/Responses.json";

        private DialogueGraphView _graphView;
        private DialogueListModel _dialogueData;
        private ResponseListModel _responseData;

        [MenuItem("Tools/Dialogue Editor")]
        public static void Open()
        {
            GetWindow<DialogueEditorWindow>("Dialogue Editor");
        }
        
        private void OnEnable()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            LoadData();
            GenerateToolbar();
            ConstructGraphView();
        }
        
        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void ConstructGraphView()
        {
            _graphView = new DialogueGraphView
            {
                name = "Dialogue Graph",
                style =
                {
                    flexGrow = 1
                }
            };

            _graphView.LoadDialogueData(_dialogueData, _responseData);
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var createNodeButton = new Button(() =>
            {
                _graphView.CreateNewDialogueNode();
            })
            {
                text = "Create Dialogue Node"
            };
            
            var reloadButton = new Button(() =>
            {
                ReloadGraph();
            })
            {
                text = "Reload"
            };

            var saveButton = new Button(() =>
            {
                _graphView.ApplyGraphToData();
                SaveData();
            })
            {
                text = "Save"
            };
            
            toolbar.Add(createNodeButton);
            toolbar.Add(reloadButton);
            toolbar.Add(saveButton);
            rootVisualElement.Add(toolbar);
        }
        
        private void ReloadGraph()
        {
            LoadData();

            if (_graphView != null)
            {
                rootVisualElement.Remove(_graphView);
            }

            ConstructGraphView();
        }
        
        private void LoadData()
        {
            if (File.Exists(DIALOGUES_PATH))
            {
                var dialogueJson = File.ReadAllText(DIALOGUES_PATH);
                _dialogueData = JsonUtility.FromJson<DialogueListModel>(dialogueJson);
            }

            if (File.Exists(RESPONSES_PATH))
            {
                var responseJson = File.ReadAllText(RESPONSES_PATH);
                _responseData = JsonUtility.FromJson<ResponseListModel>(responseJson);
            }

            if (_dialogueData == null)
            {
                _dialogueData = new DialogueListModel();
            }

            if (_dialogueData.Dialogues == null)
            {
                _dialogueData.Dialogues = new List<DialogueModel>();
            }

            if (_responseData == null)
            {
                _responseData = new ResponseListModel();
            }

            if (_responseData.Responses == null)
            {
                _responseData.Responses = new List<ResponseModel>();
            }
        }
        
        private void SaveData()
        {
            var dialogueJson = JsonUtility.ToJson(_dialogueData, true);
            var responseJson = JsonUtility.ToJson(_responseData, true);

            File.WriteAllText(DIALOGUES_PATH, dialogueJson);
            File.WriteAllText(RESPONSES_PATH, responseJson);

            AssetDatabase.Refresh();

            Debug.Log("Dialogue graph saved to JSON.");
        }
    }
}