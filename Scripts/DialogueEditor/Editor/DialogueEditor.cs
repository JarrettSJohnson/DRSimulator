﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using EventObjects;
using UnityEngine;
using UnityEditor;

using DREditor.CharacterEditor;
using DREditor.Utility;

namespace DREditor.DialogueEditor.Editor
{
    public class DialogueEditorBase : UnityEditor.Editor
    {
        protected int value;
        protected bool _directDialogue;
        protected AudioClip _sfx = null;

        public static int[] iota(int size, int value = 0)
        {
            int[] values = new int[size];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = value++;
            }
            return values;
        }

        public static T[] prependedList<T>(T[] list, T firstElement)
        {
            T[] newList = new T[list.Length + 1];
            newList[0] = firstElement;
            for (int i = 0; i < list.Length; i++)
            {
                newList[i + 1] = list[i];
            }
            return newList;
        }

        public bool IsProtagonist(Character character)
        {
            return character is Protagonist;
        }
    }

    [CustomEditor(typeof(Dialogue))]
    public class DialogueEditor : DialogueEditorBase
    {
        Dialogue dia;

        public void OnEnable()
        {
            dia = (Dialogue)target;
        }

        public override void OnInspectorGUI()
        {
            if (dia.Speakers == null)
            {
                if (Resources.Load<CharacterDatabase>("Characters/CharacterDatabase"))
                {
                    dia.Speakers = Resources.Load<CharacterDatabase>("Characters/CharacterDatabase");
                }
                else
                {
                    GUILayout.BeginVertical();
                    EditorGUILayout.LabelField("CharacterDatabase is not set.");
                    EditorGUILayout.LabelField("Create a CharacterDatabase in Resources/Characters/CharacterDatabase.asset");
                    GUILayout.EndVertical();
                    return;
                }
            }

            if (dia.Speakers.Characters == null)
            {
                EditorGUILayout.LabelField("Add at least one character in the CharacterDatabase.");
                return;
            }

            if (dia.Speakers.Characters.Count == 0)
            {
                EditorGUILayout.LabelField("Add at least one character in the CharacterDatabase.");
                return;
            }

            if (dia.Speakers.Characters.Count > 0)
            {
                foreach (var stu in dia.Speakers.Characters)
                {
                    if (stu == null)
                    {
                        EditorGUILayout.LabelField("Nullref in CharacterDatabase. Is an element empty?");
                        return;
                    }
                }
            }



            EditorStyles.textArea.wordWrap = true;
            EditorStyles.textField.wordWrap = true;
            GUI.backgroundColor = dia.Color;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(dia.DialogueName, EditorStyles.boldLabel);
            dia.Color = EditorGUILayout.ColorField(dia.Color, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField("Dialogue Nr: ", GUILayout.Width(100));
            GUI.backgroundColor = Color.white;
            dia.DialogueName = GUILayout.TextField(dia.DialogueName, GUILayout.Width(40));






            if (GUILayout.Button("Update", GUILayout.Width(80)))
            {
                var path = AssetDatabase.GetAssetPath(dia);
                var flds = path.Split('/');
                var nm = flds[flds.Length - 2];



                string tx = "";
                if (dia.Lines.Count > 0)
                {
                    tx = dia.Lines[0].Text.Substring(0, dia.Lines[0].Text.Length > 29 ? 30 : dia.Lines[0].Text.Length);
                    tx = tx.Replace('?', ' ');
                }

                string fileName = nm + "_" + dia.DialogueName + "_" + tx;

                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(dia), fileName);
                Debug.Log(fileName);
                Debug.Log(AssetDatabase.GetAssetPath(dia));
            }
            GUILayout.Space(Screen.width - 370);
            GUI.backgroundColor = dia.Color;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField("PostProcess: ", GUILayout.Width(100));
            dia.PostProcessStyle = GUILayout.TextField(dia.PostProcessStyle, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            //dia.GetCharacters();

            if (dia.Lines != null)
            {
                for (int i = 0; i < dia.Lines.Count; i++)
                {
                    var currLine = dia.Lines[i];
                    var color = Color.white;
                    
                    if (dia.Lines[i].Speaker is Student)
                    {
                        var stu = dia.Lines[i].Speaker as Student;

                        float H, S, V;
                        
                        
                        color = stu.StudentCard.Color;
                        Color.RGBToHSV(color, out H, out S, out V);
                        S = 0.3f;
                        V = 0.95f;

                        color = Color.HSVToRGB(H, S, V);
    
                    }
                    else
                    {
                        GUI.backgroundColor = Color.white;
                    }
                    
                    GUI.backgroundColor = color;
                    EditorGUILayout.BeginHorizontal("Box");
                    EditorGUILayout.BeginVertical(GUILayout.Width(120));
                    GUI.backgroundColor = dia.Color;
                    var prependedArray = prependedList(dia.GetCharacterNames(), "<No Character>");
                    currLine.SpeakerNumber = EditorGUILayout.IntPopup(currLine.SpeakerNumber, prependedArray, iota(prependedArray.Length, -1), GUILayout.Width(130));
                    currLine.Speaker = currLine.SpeakerNumber == -1 ? null : dia.Speakers.Characters[currLine.SpeakerNumber];

                    if (dia.Lines[i].Speaker) {
                        var aliasNames = new string[dia.Lines[i].Speaker.Aliases.Count + 1];
                        aliasNames[0] = "(No Alias)";

                        for (int j = 1; j < dia.Lines[i].Speaker.Aliases.Count + 1; j++)
                        {
                            aliasNames[j] = dia.Lines[i].Speaker.Aliases[j - 1].Name;
                        }
                        dia.Lines[i].AliasNumber = EditorGUILayout.IntPopup(dia.Lines[i].AliasNumber,
                            aliasNames, dia.getAliasesIntValues(dia.Lines[i].Speaker),
                                GUILayout.Width(130));
                    }
                    
                    
                    

                    
                    
                    
                        


                    if (dia.Lines[i].SFX != null)
                    {
                        for (var j = 0; j < dia.Lines[i].SFX.Count; j++)
                        {



                            EditorGUILayout.BeginHorizontal();
                            EditorGUI.BeginDisabledGroup(dia.Lines[i].SFX[j] == null);

                            if (GUILayout.Button(">", GUILayout.Width(20)))
                            {
                                PublicAudioUtil.PlayClip(dia.Lines[i].SFX[j]);
                            }

                            EditorGUI.EndDisabledGroup();

                            dia.Lines[i].SFX[j] =
                                (AudioClip)EditorGUILayout.ObjectField(dia.Lines[i].SFX[j], typeof(AudioClip), false,
                                    GUILayout.Width(76));



                            if (GUILayout.Button("x", GUILayout.Width(20)))
                            {
                                dia.Lines[i].SFX.Remove(dia.Lines[i].SFX[j]);
                            }
                            EditorGUILayout.EndHorizontal();
                        }


                    }

                    if (GUILayout.Button("Add Sound"))
                    {
                        dia.Lines[i].SFX.Add(_sfx);
                    }

                    if (dia.Lines[i].Events != null)
                    {
                        for (var j = 0; j < dia.Lines[i].Events.Count; j++)
                        {



                            EditorGUILayout.BeginHorizontal();
                            dia.Lines[i].Events[j] = (SceneEvent)EditorGUILayout.ObjectField(dia.Lines[i].Events[j], typeof(SceneEvent), false, GUILayout.Width(100));


                            if (GUILayout.Button("x", GUILayout.Width(20)))
                            {
                                dia.Lines[i].Events.Remove(dia.Lines[i].Events[j]);
                            }
                            EditorGUILayout.EndHorizontal();

                        }
                    }

                    if (GUILayout.Button("Add Event"))
                    {

                        dia.Lines[i].Events.Add(CreateInstance<SceneEvent>());
                    }




                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Automatic", GUILayout.Width(60));
                    dia.Lines[i].AutomaticLine = EditorGUILayout.Toggle(dia.Lines[i].AutomaticLine);
                    GUILayout.EndHorizontal();
                    if (dia.Lines[i].AutomaticLine)
                    {
                        EditorGUILayout.BeginHorizontal();

                        dia.Lines[i].TimeToNextLine = EditorGUILayout.FloatField(dia.Lines[i].TimeToNextLine, GUILayout.Width(60));

                        EditorGUILayout.EndHorizontal();
                    }


                    EditorGUILayout.EndVertical();

                    if (dia.Lines[i].Speaker != null && !IsProtagonist(dia.Lines[i].Speaker))
                    {


                        EditorGUILayout.BeginVertical("Box");

                        var exprs = dia.Lines[i].Speaker.Expressions.Count;

                        if (exprs < dia.Lines[i].ExpressionNumber)
                        {
                            dia.Lines[i].ExpressionNumber = 0;
                        }

                        var expressionNames = new string[dia.Lines[i].Speaker.Expressions.Count + 1];
                        expressionNames[0] = "<No change>";

                        for (int j = 1; j < dia.Lines[i].Speaker.Expressions.Count + 1; j++)
                        {
                            expressionNames[j] = dia.Lines[i].Speaker.Expressions[j - 1].Name;
                        }

                        if (dia.Lines[i].Expression != null)
                        {
                            GUIStyle expr = new GUIStyle();
                            if (dia.Lines[i].Expression.Sprite != null && dia.Lines[i].ExpressionNumber > 0)
                            {
                                var tex = dia.Lines[i].Expression.Sprite.GetTexture("_BaseMap") as Texture2D;
                                if (tex)
                                {
                                    expr.normal.background = tex;
                                }
                            }

                            EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100),
                                GUILayout.Height(100));

                        }







                        dia.Lines[i].ExpressionNumber = EditorGUILayout.IntPopup(dia.Lines[i].ExpressionNumber,
                            expressionNames, dia.getExpressionIntValues(dia.Lines[i].Speaker), GUILayout.Width(100));


                        if (dia.Lines[i].ExpressionNumber > 0)
                        {
                            dia.Lines[i].Expression =
                                dia.Lines[i].Speaker.Expressions[dia.Lines[i].ExpressionNumber - 1];
                        } else {
                            dia.Lines[i].Expression = new Expression();
                        }


                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        

                        EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(108),
                            GUILayout.Height(100));
                    }



                    GUI.backgroundColor = Color.white;
                    dia.Lines[i].Text = EditorGUILayout.TextArea(dia.Lines[i].Text, GUILayout.Height(125), GUILayout.Width(Screen.width - 310));

                    GUI.backgroundColor = dia.Color;

                    EditorGUILayout.BeginVertical();
                    if (dia.Lines.Count > 1)
                    {
                        if (GUILayout.Button("-", GUILayout.Width(20)) && dia.Lines.Count > 1)
                        {
                            GUI.FocusControl(null);
                            dia.Lines.Remove(dia.Lines[i]);

                        }
                    }

                    if (i > 0)
                    {
                        if (GUILayout.Button("ʌ", GUILayout.Width(20)) && i > 0)
                        {
                            {
                                GUI.FocusControl(null);
                                var line = dia.Lines[i - 1];

                                dia.Lines[i - 1] = dia.Lines[i];
                                dia.Lines[i] = line;
                            }
                        }
                    }
                    

                    if (i < dia.Lines.Count - 1)
                    {
                        if (GUILayout.Button("v", GUILayout.Width(20)))
                        {
                            GUI.FocusControl(null);
                            var line = dia.Lines[i + 1];

                            dia.Lines[i + 1] = dia.Lines[i];
                            dia.Lines[i] = line;
                        }
                    }
                    
                    GUILayout.FlexibleSpace();
                    
                    
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        dia.Lines.Insert(i+1, new Line());
                    }
                    
                    



                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();




                }
            }




            if (dia.Choices != null)
            {
                for (int i = 0; i < dia.Choices.Count; i++)
                {
                    //string[] names = new string[diaCol.Dialogues.Count];

                    GUILayout.BeginHorizontal("Box");
                    //GUILayout.FlexibleSpace();
                    GUILayout.Label("Choice " + (i + 1).ToString(), GUILayout.Width(80));

                    dia.Choices[i].ChoiceText = EditorGUILayout.TextField(dia.Choices[i].ChoiceText, GUILayout.Width(Screen.width - 343));

                    if (dia.Choices[i].NextDialogue == null)
                    {
                        GUI.backgroundColor = Color.cyan;
                    }
                    dia.Choices[i].NextDialogue =
                        (Dialogue)EditorGUILayout.ObjectField(dia.Choices[i].NextDialogue, typeof(Dialogue), true,
                            GUILayout.Width(100));

                    GUI.backgroundColor = dia.Color;


                    if (dia.Choices[i].NextDialogue != null)
                    {
                        string[] nextDialogueTexts = new string[dia.Choices[i].NextDialogue.Lines.Count];
                        int[] optValues = new int[dia.Choices[i].NextDialogue.Lines.Count];

                        for (int j = 0; j < dia.Choices[i].NextDialogue.Lines.Count; j++)
                        {
                            nextDialogueTexts[j] = dia.Choices[i].NextDialogue.Lines[j].Text;
                            optValues[j] = value;
                            value++;
                        }

                        value = 0;


                        GUI.backgroundColor = Color.white;


                        GUI.backgroundColor = dia.Choices[i].NextDialogue.Color;


                        dia.Choices[i].NextIndexInDialogue = EditorGUILayout.IntPopup(dia.Choices[i].NextIndexInDialogue, nextDialogueTexts, optValues, GUILayout.Width(100));





                    }
                    GUILayout.EndHorizontal();



                    //int[] optValues = new int[diaCol.Dialogues.Count];


                }

                //dia.choices[i].nextDialogue = EditorGUILayout.TextField(dia.choices[i].nextDialogue, GUILayout.Width(100));

                GUI.backgroundColor = dia.Color;

                if (dia.Choices.Count > 0)
                {
                    if (GUILayout.Button("Remove Choices", GUILayout.Width(130)))
                    {
                        dia.Choices.Clear();
                    }
                }

            }

            if (dia.Variable.Enabled)
            {
                GUILayout.BeginVertical("Box", GUILayout.Width(400));
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = dia.Color;
                EditorGUILayout.LabelField("If ", GUILayout.Width(80));



                dia.Variable.BoolVariable =
                    (BoolWithEvent)EditorGUILayout.ObjectField(dia.Variable.BoolVariable, typeof(BoolWithEvent), false, GUILayout.Width(200));

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("True: ", GUILayout.Width(80));

                if (dia.Variable.NextDialogueTrue == null)
                {
                    GUI.backgroundColor = Color.cyan;
                }
                dia.Variable.NextDialogueTrue = (Dialogue)EditorGUILayout.ObjectField(dia.Variable.NextDialogueTrue, typeof(Dialogue), false, GUILayout.Width(200));

                GUI.backgroundColor = dia.Color;

                if (dia.Variable.NextDialogueTrue != null)
                {
                    string[] nextDialogueTextsTrue = new string[dia.Variable.NextDialogueTrue.Lines.Count];
                    int[] optValues2 = new int[dia.Variable.NextDialogueTrue.Lines.Count];

                    for (int j = 0; j < dia.Variable.NextDialogueTrue.Lines.Count; j++)
                    {
                        nextDialogueTextsTrue[j] = dia.Variable.NextDialogueTrue.Lines[j].Text;
                        optValues2[j] = value;
                        value++;
                    }


                    dia.Variable.NextIndexInDialogueTrue = EditorGUILayout.IntPopup(
                        dia.Variable.NextIndexInDialogueTrue, nextDialogueTextsTrue, optValues2, GUILayout.Width(100));

                }

                value = 0;

                EditorGUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("False: ", GUILayout.Width(80));

                if (dia.Variable.NextDialogueFalse == null)
                {
                    GUI.backgroundColor = Color.cyan;
                }

                dia.Variable.NextDialogueFalse = (Dialogue)EditorGUILayout.ObjectField(dia.Variable.NextDialogueFalse, typeof(Dialogue), false, GUILayout.Width(200));

                GUI.backgroundColor = dia.Color;

                if (dia.Variable.NextDialogueFalse != null)
                {
                    string[] nextDialogueTextsFalse = new string[dia.Variable.NextDialogueFalse.Lines.Count];
                    int[] optValues3 = new int[dia.Variable.NextDialogueFalse.Lines.Count];

                    for (int j = 0; j < dia.Variable.NextDialogueFalse.Lines.Count; j++)
                    {
                        nextDialogueTextsFalse[j] = dia.Variable.NextDialogueFalse.Lines[j].Text;
                        optValues3[j] = value;
                        value++;
                    }

                    dia.Variable.NextIndexInDialogueFalse = EditorGUILayout.IntPopup(
                        dia.Variable.NextIndexInDialogueFalse, nextDialogueTextsFalse, optValues3, GUILayout.Width(100));
                }

                value = 0;

                GUILayout.EndHorizontal();


                GUI.backgroundColor = dia.Color;


                if (GUILayout.Button("Remove Condition", GUILayout.Width(130)))
                {
                    dia.Variable.BoolVariable = null;
                    dia.Variable.Enabled = false;
                }

                GUILayout.EndVertical();

            }




            if (dia.DirectTo.Enabled)
            {
                GUILayout.BeginVertical("Box", GUILayout.Width(400));
                GUILayout.BeginHorizontal();

                GUILayout.Label("Next Dialogue:", GUILayout.Width(90));

                if (dia.DirectTo.NewDialogue == null)
                {
                    GUI.backgroundColor = Color.cyan;
                }


                dia.DirectTo.NewDialogue =
                    (Dialogue)EditorGUILayout.ObjectField(dia.DirectTo.NewDialogue, typeof(Dialogue), false,
                        GUILayout.Width(180));
                GUI.backgroundColor = dia.Color;

                if (dia.DirectTo.NewDialogue != null)
                {
                    string[] nextDialogueTexts = new string[dia.DirectTo.NewDialogue.Lines.Count];
                    int[] optValues = new int[dia.DirectTo.NewDialogue.Lines.Count];

                    for (int j = 0; j < dia.DirectTo.NewDialogue.Lines.Count; j++)
                    {
                        nextDialogueTexts[j] = dia.DirectTo.NewDialogue.Lines[j].Text;
                        optValues[j] = value;
                        value++;
                    }


                    dia.DirectTo.NewDialogueIndex = EditorGUILayout.IntPopup(
                        dia.DirectTo.NewDialogueIndex, nextDialogueTexts, optValues, GUILayout.Width(100));

                }

                value = 0;


                GUILayout.EndHorizontal();

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    dia.DirectTo.NewDialogue = null;
                    dia.DirectTo.Enabled = false;

                }

                GUILayout.EndVertical();
            }

            if (dia.SceneTransition.Enabled)
            {
                GUILayout.BeginVertical("Box", GUILayout.Width(400));
                GUILayout.BeginHorizontal();

                GUILayout.Label("Name of next Scene:", GUILayout.Width(130));



                dia.SceneTransition.Scene = GUILayout.TextField(dia.SceneTransition.Scene);



                GUILayout.EndHorizontal();

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    dia.SceneTransition.Scene = string.Empty;
                    dia.SceneTransition.Enabled = false;

                }

                GUILayout.EndVertical();
            }






            GUILayout.BeginHorizontal();

            if (dia.Choices.Count == 0 && !dia.Variable.Enabled && !dia.DirectTo.Enabled && !dia.SceneTransition.Enabled)
            {
                if (dia.Speakers == null)
                {
                    if (Resources.Load<CharacterDatabase>("Characters/CharacterDatabase"))
                    {
                        dia.Speakers = Resources.Load<CharacterDatabase>("Characters/CharacterDatabase");
                    }
                    else
                    {
                        GUILayout.BeginVertical();
                        EditorGUILayout.LabelField("CharacterDatabase is not set.");
                        EditorGUILayout.LabelField("Create a CharacterDatabase in Resources/Characters/CharacterDatabase.asset");
                        GUILayout.EndVertical();
                    }



                }
                else
                {
                    if (GUILayout.Button("New Line", GUILayout.Width(100)))
                    {
                        dia.Lines.Add(new Line());
                    }
                }




                if (dia.Lines.Count > 0)
                {
                    if (GUILayout.Button("Add Choice", GUILayout.Width(100)))
                    {
                        dia.Choices.Add(new Choice());
                        dia.Choices.Add(new Choice());
                        dia.Choices.Add(new Choice());
                    }
                }

                if (dia.Lines.Count > 0)
                {
                    if (GUILayout.Button("Add Condition", GUILayout.Width(100)))
                    {
                        dia.Variable.Enabled = true;
                    }

                    if (GUILayout.Button("Direct to...", GUILayout.Width(100)))
                    {
                        dia.DirectTo.Enabled = true;
                    }

                    if (GUILayout.Button("Enter Scene", GUILayout.Width(100)))
                    {
                        dia.SceneTransition.Enabled = true;
                    }

                }


            }

            GUILayout.EndHorizontal();



            GUILayout.Space(30);


            EditorUtility.SetDirty(dia);
        }


    }
}


