Система была #Сделано  в первую очередь, и пока что меч всё ещё имеет тенденцию врезаться в землю, что надо ~~#.Доделать.~~
	Такое происходит только при подготовке к удару, так как нет проверок.
	Есть смысл сделать перестановки и пересчёты. прямо в Block() и Swing(), а не в Incoming().
	Вариант: При потенциальном столкновении с какой-то статичной поверхностью - Брать Hit из Raycast'а, брать нормаль поверхности, и делать Vector3.CastOnPlain (Как-то так). И ставить оружие перед ударом параллельно земле, таким образом.
		В итоге проблема решилась сама собой, когда я добавил привязку меча к руке, а не к коллайдеру.

Так же надо ~~#.Доделать~~ прикрепление меча к управляющей конечности, для адекватности анимаций. Сейчас прикрепление идёт ко всему vital коллайдеру.

Ещё надо ~~#.Доделать~~ один момент, связанный с Incoming - Переместить его в StateMachine сам по себе, и дальше пусть он вызывает событие в состояния. Это сделает декомпозицию, и решит проблему появляющихся GameObject'ов

И ЕЩЁ надо Комбо ~~#.ВозможноДоделать~~. Там почти всё готово к этому. А ещё лучше перенести систему комбо в MeleeFighter

~~#.Доделать~~ : Блокировка очень быстрых объектов. 

~~#.Проблемно~~ : Равномерное притягивание к руке. Прямо сейчас, если вырубить притягивание - выглядит круто. Пока что так есть смысл и оставить.
	Оказалось, что там просто ничерта видно не было. Я сделал так, чтобы ближайшей точкой при установлении start и end всегда был start (Рукоять), и всё стало нормально. Тем не менее, меч всё ещё иногда улетает прочь, так что притяжение (Рукоятью) сделать надо.

~~#.Доделать~~ CurrentActivity может быть null в состояниях SwordFighter'а. Это нужно исправить.

Потом - [[Базовая Логика Игры]]

~~#.Доделать~~ Подрихтовать значения, сделать распределение возможных ударов (Наболее высокое - вверху, с боков - реже) с учётом текущего положения меча а также комбо (Если есть, у комбо будет реально высокий вес). Получится эдакий Utility AI для мечебоя.
Это нужно, так как сейчас мечебойцы хреначат всё время от одной и той же точки, которая находится у земли. Ударяются мечом об землю, и не бьют в итоге вовсе.

~~#.Доделать~~ частота ударов - сейчас мечебойцы бьют, почему-то, вообще постоянно, Без учёта Cooldown, указанного в оружии.

~~#.Доделать~~ Толку от блока нет - урон всё равно проходит. Надо научить правильно ставить блок так, чтобы меч не долетал до Vital. Можно использовать края Collider'а, так как мечебоец всегда смотрит на своего противника, относительно краёв позиционировать блок, а не просто об центр.
	Это применимо только к игроку.

~~#.Доделать~~ Угол поворота заставляет меч набирать бешенную скорость, что создаёт не нужные и лишние криты.

~~#.Доделать~~ Первая итерация комбо не проходит

#Доделать :
- Придумать систему, при которой этот Swordfighter будет ОЩУЩАТЬСЯ. Хорошо, чётко, и чтобы сражаться с ним было весело. Это может/должно включать в себя:
- [ ] Работу со звуком, надо много звуков вообще:
	- [ ] Для парирования
	- [ ] Для попадания
	- [ ] Для отбивания блоком
- [ ] Более живой вид управления оружием (Использование Animation Curve)
- [ ] Интересные комбо
- [ ] Сбалансированность против игрока
- [ ] #Проблемно Нормальный учёт ударов. Иногда игрок может попасть по врагу даже если враг блок ставит. А иногда по игроку происходит попадание, когда тот блок ставит.
	- Тут надо думать.
- [ ] {Точка бифуркации, сначала надо сделать то, что уже есть.}