Bonjour cher confrère ! 

Dans cet exercice vous devez écrire une classe qui permette la gestion de stock de produits.

================================
Modélisation
================================

Un stock est relatif à plusieurs produits.
Un produit est identifé par un EAN (8 caractères alphanumériques)
Un mouvement (ou flux) de produits contient une date, un libellé, et un ou plusieurs ensemble (produit,quantité).

================================
Fonctionnalités générales
================================
Nous devons pouvoir pour ce stock : 
 t ajouter des quantités de produits (par exemple lors d'achat aux fournisseurs) 
 t supprimer des quantités de produits (par exemple lors de commande clients) 
 t ajouter plusieurs mouvements de stock sur plusieurs produits à une date (mais avec un seul libellé)
 t connaitre le stock d'un produit à n'importe quelle date dans le passé
 t connaitre les variations de stock d'un produit pendant n'importe quelle période(intervalle de date)
    Si un stock est à 10 à une date d1, et à 20 à une date d2, alors la variation entre d1 et d2 est +10.
 t connaitre le stock actuel d'un produit
 t connaitre les produits actuellement en stock (les stocks négatifs sont considérés comme nuls)
 t connaitre le nombre total de produit dans le stock (les stocks négatifs sont considérés comme nuls)
 t régulariser le stock d'un produit (voir inventaire)

Les mouvements ne se suivent pas forcément dans le temps, 
t mais il n'est pas possible d'ajouter un mouvement à une date antérieure ou égale à celle d'un inventaire.

===============================
Inventaire
===============================
De temps à autre une régulation du stock (un inventaire) est fait sur un produit. 
t On doit pouvoir alors créer un mouvement pour un produit qui fixe le stock à une valeur donnée à la date du jour.

===============================
Détails
===============================
t L'ajout dans le stock d'un produit non existant crée le produit.
t Il est possible d'avoir des quantités négatives. 
t Les mouvements d'inventaires sont uniques et ne peuvent pas être négatifs.
i Si lors de l'insertion de ensemble de mouvements, l'une des valeurs n'est pas autorisée (pour cause d'inventaire postérieur), 
  l'ensemble des opérations est rejetée.
i Les heures des mouvements sont ignorées.
? A la création du stock, aucun produit n'est présent.

================================
Implémentation
================================
Votre implémentation devra contenir à minima une classe de gestion de stock, et une classe produit.
Vous n'êtes pas limité à ces deux classes, vous pouvez si nécessaire en créer d'autre.
La classe de gestion de stock devra contenir à minima les opérations suivantes : 
 - Mouvement unique pour un produit à une date
 - Mouvement pour un ou plusieur produit à une date
 - Mouvement d'inventaire pour un produit
 - Récupération de la valeur du stock à une date
 - Récupération de la variation du stock sur une intervalle de date
 - Liste des produits en stock

 Voici à titre d'exemple une signature possible de l'ajout d'un mouvement sur un produit.
public void MettreUnVraiNomDeFonction(DateTime date,string label,Product product,int quantity)

Vous ne devez ajouter aucun nugget tiers à votre bibliothèque métier (mais vous pouvez en ajouter à votre bibliothèque de test)

================================
Performance
================================
Le système est considéré comme assez rapide pour ne pas avoir à stocker de valeurs calculées.
Cependant, il ne faudra pas faire plus de calcul que nécessaire.

================================
Exemple
================================

pour les opérations suivantes : 

+------------+------------+----------+----------+
|   Date     |  Libellé   | Quantité | produit  |
+------------+------------+----------+----------+
| 01/01/2020 | Achat N°1  |       10 | EAN00001 |
| 01/01/2020 | Achat N°2  |       10 | EAN00002 |
| 01/01/2020 | Achat N°3  |       10 | EAN00003 |
| 02/01/2020 | Cmd N°1    |       -3 | EAN00001 |
| 02/01/2020 | Cmd N°1    |       -3 | EAN00002 |
| 02/01/2020 | Cmd N°1    |       -3 | EAN00003 |
| 03/01/2020 | Cmd N°2    |       -1 | EAN00001 |
| 03/01/2020 | Cmd N°2    |      -10 | EAN00002 |
| 04/01/2020 | inventaire |        1 | EAN00002 |
+------------+------------+----------+----------+

nous obtiendrons les valeurs de stocks suivantes


+------------+----------+----------+----------+-------+
|            | EAN00001 | EAN00002 | EAN00003 | Total |
+------------+----------+----------+----------+-------+
| 01/01/2020 |       10 |       10 |       10 |    30 |
| 02/01/2020 |        7 |        7 |        7 |    21 |
| 03/01/2020 |        6 |       -3 |        7 |    13 |
| 04/01/2020 |        6 |        1 |        7 |    13 |
+------------+----------+----------+----------+-------+

================================
Valorisation de votre solution
================================
1 - Un solution qui fonctionne
2 - Nommage des éléments
3 - Non-répétition
4 - Utilisation des structures de données adéquates
5 - Test unitaires
6 - Respect des principes SOLID 
