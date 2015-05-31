# Pillars of Eternity Quest Editor (im entstehen)

Basiert momentan rein auf [Polymer](www.polymer-project.org)

Das ganze ist in der Experimentierphase.
Für UI Verbesserungen und Ideen bitte ein Ticket anlegen

## Fähigkeiten
* Anzeigen der Struktur von Quests und Conversations
  * Anzeigen von Bedingungen zum Betreten einer Node und Scriptaufrufen beim Passieren einer Node (Die Dokumentation, was die Funktionen bewirken ist noch nicht erstellt)
* Verknüpfen von Questnodes/ Talknodes via Drag und Drop
* Editierern und Hinzufügen von Comments
* XML Lesen und mit Änderungen wieder ausgeben (soweit Features für die Tags vorhanden sind)

Rest: Muss noch gebaut werden

## Demo
http://xaratas.github.io

## Links
Dokumentation des Quest-/Conversationsystems http://pillarsofeternity.gamepedia.com/Modding
XSD um Quest- und Conversationdateien zu validieren: https://bitbucket.org/ckirschner/poe-modding-framework/src/9c50d164c2ca74162dc39086f97702acd0b9e662/QuestModResources/quest2.xsd?at=QuestModding
https://bitbucket.org/ckirschner/poe-modding-framework/src/6f1d5264711c4eafc5ad46641d7829d69966e9cf/QuestModResources/conversation.xsd

### Abhängigkeiten
Keine, es ist alles im Git enthalten.
Zum Entwickeln hilft aber [bower](bower.io) installiert zu haben.