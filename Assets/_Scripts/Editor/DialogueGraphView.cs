using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Editor
{
    public class DialogueGraphView : GraphView
    {
        private DialogueListModel _dialogueData;
        private ResponseListModel _responseData;

        private readonly Dictionary<int, DialogueNodeView> _nodesByDialogueId = new();
        
        public DialogueGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            AddGridBackground();
            AddStyles();
        }
        
        private void AddGridBackground()
        {
            var gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
            gridBackground.SendToBack();
        }

        private void AddStyles()
        {
            var styleSheet = EditorGUIUtility.Load("Dialogue/DialogueGraphViewStyles.uss") as StyleSheet;
            styleSheets.Add(styleSheet);
        }
        
        public void LoadDialogueData(DialogueListModel dialogueData, ResponseListModel responseData)
        {
            _dialogueData = dialogueData;
            _responseData = responseData;

            DeleteElements(graphElements.ToList());
            _nodesByDialogueId.Clear();

            CreateNodesFromDialogues();
            CreateEdgesFromResponses();
        }

        public void CreateNewDialogueNode()
        {
            var nextDialogueId = GetNextDialogueId();

            var dialogue = new DialogueModel
            {
                Id = nextDialogueId,
                Text = "New dialogue",
                Type = "Neutral",
                PossibleResponse = new List<int>()
            };

            _dialogueData.Dialogues.Add(dialogue);

            var node = CreateDialogueNodeView(dialogue, new Vector2(250, 200));
            AddElement(node);
            _nodesByDialogueId.Add(dialogue.Id, node);
        }

        public void ApplyGraphToData()
        {
            foreach (var node in nodes.ToList().OfType<DialogueNodeView>())
            {
                if (node.DialogueModel == null)
                {
                    continue;
                }

                node.DialogueModel.Id = node.DialogueId;
                node.DialogueModel.Text = node.DialogueText;

                if (node.DialogueModel.PossibleResponse == null)
                {
                    node.DialogueModel.PossibleResponse = new List<int>();
                }

                node.DialogueModel.PossibleResponse.Clear();

                foreach (var output in node.outputContainer.Children().OfType<Port>())
                {
                    if (output.userData is not ResponseModel response)
                    {
                        continue;
                    }

                    if (!node.DialogueModel.PossibleResponse.Contains(response.Id))
                    {
                        node.DialogueModel.PossibleResponse.Add(response.Id);
                    }

                    var connectedEdge = output.connections.FirstOrDefault();

                    if (connectedEdge?.input?.node is DialogueNodeView targetNode)
                    {
                        response.NextDialogueId = targetNode.DialogueId;
                    }
                    else
                    {
                        response.NextDialogueId = 0;
                    }
                }
            }
        }
        
        private void CreateNodesFromDialogues()
        {
            foreach (var dialogue in _dialogueData.Dialogues)
            {
                if (dialogue.PossibleResponse == null)
                {
                    dialogue.PossibleResponse = new List<int>();
                }

                var position = new Vector2(
                    250 + dialogue.Id * 300,
                    200
                );

                var node = CreateDialogueNodeView(dialogue, position);
                AddElement(node);
                _nodesByDialogueId[dialogue.Id] = node;
            }
        }
        
        private DialogueNodeView CreateDialogueNodeView(DialogueModel dialogue, Vector2 position)
        {
            var node = new DialogueNodeView
            {
                title = $"Dialogue {dialogue.Id}",
                Guid = System.Guid.NewGuid().ToString(),
                DialogueId = dialogue.Id,
                DialogueText = dialogue.Text,
                DialogueModel = dialogue
            };

            node.SetPosition(new Rect(position, new Vector2(300, 250)));

            var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            var idField = new IntegerField("Id")
            {
                value = dialogue.Id
            };

            idField.RegisterValueChangedCallback(evt =>
            {
                node.DialogueId = evt.newValue;
                dialogue.Id = evt.newValue;
                node.title = $"Dialogue {evt.newValue}";
            });

            var textField = new TextField("Text")
            {
                value = dialogue.Text,
                multiline = true
            };

            textField.RegisterValueChangedCallback(evt =>
            {
                node.DialogueText = evt.newValue;
                dialogue.Text = evt.newValue;
            });

            var typeField = new TextField("Type")
            {
                value = dialogue.Type
            };

            typeField.RegisterValueChangedCallback(evt =>
            {
                dialogue.Type = evt.newValue;
            });

            node.mainContainer.Add(idField);
            node.mainContainer.Add(textField);
            node.mainContainer.Add(typeField);

            var addResponseButton = new Button(() =>
            {
                var response = new ResponseModel
                {
                    Id = GetNextResponseId(),
                    Text = "New response",
                    NextDialogueId = 0
                };

                _responseData.Responses.Add(response);
                dialogue.PossibleResponse.Add(response.Id);

                AddResponsePort(node, response);
            })
            {
                text = "Add Response"
            };

            node.titleContainer.Add(addResponseButton);

            foreach (var responseId in dialogue.PossibleResponse)
            {
                var response = GetResponseById(responseId);

                if (response == null)
                {
                    continue;
                }

                AddResponsePort(node, response);
            }

            node.RefreshExpandedState();
            node.RefreshPorts();

            return node;
        }
        
        private void CreateEdgesFromResponses()
        {
            foreach (var outputPort in _nodesByDialogueId.Values.SelectMany(sourceNode => sourceNode.outputContainer.Children().OfType<Port>()))
            {
                if (outputPort.userData is not ResponseModel response)
                {
                    continue;
                }

                if (response.NextDialogueId == 0)
                {
                    continue;
                }

                if (!_nodesByDialogueId.TryGetValue(response.NextDialogueId, out var targetNode))
                {
                    continue;
                }

                var inputPort = targetNode.inputContainer.Children().OfType<Port>().FirstOrDefault();

                if (inputPort == null)
                {
                    continue;
                }

                var edge = outputPort.ConnectTo(inputPort);
                AddElement(edge);
            }
        }
        
        private Port GeneratePort(DialogueNodeView node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }
        
        private void AddResponsePort(DialogueNodeView node, ResponseModel response)
        {
            var generatedPort = GeneratePort(node, Direction.Output);
            generatedPort.portName = "";
            generatedPort.userData = response;

            var responseTextField = new TextField
            {
                value = response.Text
            };

            responseTextField.RegisterValueChangedCallback(evt =>
            {
                response.Text = evt.newValue;
            });

            generatedPort.contentContainer.Add(responseTextField);

            var deleteButton = new Button(() =>
            {
                if (node.DialogueModel != null && node.DialogueModel.PossibleResponse != null)
                {
                    node.DialogueModel.PossibleResponse.Remove(response.Id);
                }

                _responseData.Responses.Remove(response);
                node.outputContainer.Remove(generatedPort);

                node.RefreshPorts();
                node.RefreshExpandedState();
            })
            {
                text = "X"
            };

            generatedPort.contentContainer.Add(deleteButton);

            node.outputContainer.Add(generatedPort);

            node.RefreshPorts();
            node.RefreshExpandedState();
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private DialogueModel GetDialogueById(int dialogueId)
        {
            return _dialogueData.Dialogues.Find(dialogue => dialogue.Id == dialogueId);
        }

        private ResponseModel GetResponseById(int responseId)
        {
            return _responseData.Responses.Find(response => response.Id == responseId);
        }

        private int GetNextDialogueId()
        {
            var nextId = 1;

            foreach (var dialogue in _dialogueData.Dialogues)
            {
                if (dialogue.Id >= nextId)
                {
                    nextId = dialogue.Id + 1;
                }
            }

            return nextId;
        }

        private int GetNextResponseId()
        {
            var nextId = 1;

            foreach (var response in _responseData.Responses.Where(response => response.Id >= nextId))
            {
                nextId = response.Id + 1;
            }

            return nextId;
        }
    }
}