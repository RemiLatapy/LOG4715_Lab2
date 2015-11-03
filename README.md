# LOG4715_Lab2

* Modif général :
** CarController
rank : rank of the car
raceManager : reference to the race manager 
	- StartCoroutine(raceManager.DisplayText("textToDisplay", timeOfDisplayMS));
numberOfCars : total number of cars (8 by default because of player 2)
void OnGui() : useful to debug
bool IsPlayer() : check if the current car is a user car.
