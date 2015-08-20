# Pillars of Eternity Quest Editor (im entstehen) v0.3.0

Basiert momentan rein auf [Polymer](www.polymer-project.org)

XML für Conversation, Quest und Stringtable vollständig bis Spielversion 1.0.6. Ausgabe kann für Spiele bis 1.0.4 umgeschaltet werden. (constants.html)
Verknüpfen von Quest und Conversation zu Stringtable vollständig.
Für UI Verbesserungen und Ideen bitte ein Ticket anlegen.

## Fähigkeiten
* Anzeigen der Struktur von Quests und Conversations
  * Anzeigen von Bedingungen zum Betreten einer Node und Scriptaufrufen beim Passieren einer Node (Eine Kurze Beschreibung, was die Funktionen bewirken ist in title Attributen auf den Namen und Icons hinterlegt)
* Verknüpfen von Questnodes/ Talknodes via Drag und Drop
* Editierern und Hinzufügen von Comments
* XML Lesen und mit Änderungen wieder ausgeben für Quest, Conversation und Stringtable
* Verknüpfen von Conversations und Quests mit ihren Stringtable Dateien
* Highlight ausgewählter Nodes
* Highlight ein- und ausgehender Links in verschiedenen Farben (grün bzw. blau)
* Anzeigen und editieren der Texte für die ausgewählte Node
* Editieren aller Werte auf Quest Nodes
* Editieren von Conditions in allen Belangen, ohne Validierung der Eingaben.
* Editieren von Scriptcalls in allen Belangen, ohne Validierung der Eingaben.
* Editieren von QuestEvents in allen Belangen, ohne Validierung der Eingaben.
* Anzeigen der redenden und zuhörenden NPCs
* Ausgeben der editierten Files auf der Konsole (ohne Formatierung)
* Anzeige der Quest Events


Rest: Muss noch gebaut werden :)

## Demo
http://xaratas.github.io

## Links
Dokumentation des Quest-/Conversationsystems http://pillarsofeternity.gamepedia.com/Modding


Dokumentation Conditions, Scripts, NPCs 
* http://pillarsofeternity.gamepedia.com/Modding/Quest/
* http://pillarsofeternity.gamepedia.com/Modding/Conditionals
* ttp://pillarsofeternity.gamepedia.com/Modding/Scripts


XSD um Quest- und Conversationdateien zu validieren: 
* https://bitbucket.org/ckirschner/poe-modding-framework/src/9c50d164c2ca74162dc39086f97702acd0b9e662/QuestModResources/quest2.xsd?at=QuestModding
* https://bitbucket.org/ckirschner/poe-modding-framework/src/6f1d5264711c4eafc5ad46641d7829d69966e9cf/QuestModResources/conversation.xsd


### Abhängigkeiten
Polymer mind 1.1.0


[bower](bower.io) um die referenzierten Polymer Elemente zu laden.


optional npm, gulp für jslint
