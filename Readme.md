# Pillars of Eternity Quest Editor (im entstehen) v0.2.9

Basiert momentan rein auf [Polymer](www.polymer-project.org)

XML für Conversation, Quest (bis auf Events) und Stringtable vollständig. Bis Version 1.0.4, 1.0.5 hat die xml Dateien erweitert.
Verknüpfen von Quest und Conversation zu Stringtable vollständig.
Für UI Verbesserungen und Ideen bitte ein Ticket anlegen.

## Fähigkeiten
* Anzeigen der Struktur von Quests und Conversations
  * Anzeigen von Bedingungen zum Betreten einer Node und Scriptaufrufen beim Passieren einer Node (Die Dokumentation, was die Funktionen bewirken ist noch nicht erstellt)
* Verknüpfen von Questnodes/ Talknodes via Drag und Drop
* Editierern und Hinzufügen von Comments
* XML Lesen und mit Änderungen wieder ausgeben für Quest, Conversation und Stringtable
* Verknüpfen von Conversation mit ihren Stringtable Dateien
* Highlight ausgewählter Nodes
* Highlight ein- und ausgehender Links in verschiedenen Farben
* Anzeigen und editieren der Texte für die ausgewählte Node
* Editieren von Playtype, Persistence, DisplayType, Experiencetype, Experience Level und Experience Weight
* Editieren von Conditions in allen belangen, ohne Validierung der Eingaben.
* Anzeigen der redenden und zuhörenden NPCs
* Ausgeben der editierten Files auf der Konsole (ohne Formatierung)
* Hilfetexte, was die Conditionfunktionen bewirken


Rest: Muss noch gebaut werden :)

## Demo
http://xaratas.github.io

## Links
Dokumentation des Quest-/Conversationsystems http://pillarsofeternity.gamepedia.com/Modding


Dokumentation Conditions und NPCs http://pillarsofeternity.gamepedia.com/Modding/Quest/


XSD um Quest- und Conversationdateien zu validieren: https://bitbucket.org/ckirschner/poe-modding-framework/src/9c50d164c2ca74162dc39086f97702acd0b9e662/QuestModResources/quest2.xsd?at=QuestModding


https://bitbucket.org/ckirschner/poe-modding-framework/src/6f1d5264711c4eafc5ad46641d7829d69966e9cf/QuestModResources/conversation.xsd


### Abhängigkeiten
[bower](bower.io) um die referenzierten Polymer Elemente zu laden.
optional npm, gulp für jslint
