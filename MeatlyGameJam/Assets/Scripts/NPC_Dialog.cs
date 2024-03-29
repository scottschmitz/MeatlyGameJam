﻿using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Xml;

public class NPC_Dialog : MonoBehaviour {

	public GameObject questObject;
	public Sprite happySprite;
	
	public bool questObjectAppear = true;
	public bool questObjectDisappear = false;
	
	public string xmlFileName;

	private bool isTalking = false;	
	private bool isTextScrolling = false;
	
	private string[] activeLines;
	private string[] questLines;
	private string[] completeLines;
	
	private int currentLine = 0;
	private int textSpeed = 2;
	
	void Start() {
	
		TextAsset textAsset = (TextAsset) Resources.Load (xmlFileName);
	
		XmlDocument xmlDoc = new XmlDocument();	
		xmlDoc.LoadXml (textAsset.text);
		
		XmlNodeList xnList = xmlDoc.SelectNodes("scene/quest/line");
		questLines = new string[xnList.Count];
		for(int i=0; i < xnList.Count; i++) {
			questLines[i] = xnList[i].InnerText;
		}
		
		xnList = xmlDoc.SelectNodes("scene/complete/line");
		completeLines = new string[xnList.Count];
		for(int i=0; i < xnList.Count; i++) {
			completeLines[i] = xnList[i].InnerText;
		}
	}
	
	void Update () {
		if (isTalking) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				if (isTextScrolling) {
					// display the full line
					isTextScrolling = false;
					HUD._instance.showDialogue(activeLines[currentLine]);
				}
				else {
					if (currentLine < activeLines.Length - 1) {
						currentLine++;
						isTextScrolling = true;
						StartCoroutine(scrollText());
					}
					else {
						currentLine = 0;
						isTalking = false;
						isTextScrolling = false;
						
						// clear out the dialogue
						HUD._instance.showDialogue("");
						
						// draw the path to the quest if we havent completed it already
						if (! GameManager.Instance.getQuestComplete()) {
							if(questObjectAppear) {
								questObject.GetComponent<Renderer>().enabled = true;
								questObject.GetComponent<PolygonCollider2D>().enabled = true;
							}
							
							if(questObjectDisappear) {
								Destroy(questObject);
							}
						}
						
						
						// allow the player to move again
						GameManager.Instance.enablePlayer();
					}
				}
			}
		}
	}
	
	void OnTriggerEnter2D (Collider2D aCollider) {
	
		if (aCollider.gameObject.tag == "Player") {
		
			if (GameManager.Instance.getQuestComplete()) {
				activeLines =  completeLines;
				GetComponent<SpriteRenderer>().sprite = happySprite;
			}
			else {
				activeLines = questLines;
			}
		
			GameManager.Instance.disablePlayer();
		
			isTalking = true;
			isTextScrolling = true;
		
			StartCoroutine(scrollText());
		}
	}
	
	IEnumerator scrollText() {
	
		string displayText = "";
		
		for (int i = 0; i < activeLines[currentLine].Length && isTextScrolling; i++) {
			displayText += activeLines[currentLine][i];
			//Debug.Log (displayText);
			HUD._instance.showDialogue(displayText);
			yield return new WaitForSeconds(1/textSpeed);		
		}
		
		isTextScrolling = false;
	}
}
