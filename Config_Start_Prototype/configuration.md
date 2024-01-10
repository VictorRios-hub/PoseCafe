# Plug and play configuration

Suivre les étapes suivantes pour le lancement automatique du script python startProcess.py lors de l'ouverture de session d'un utilisateur.

## Etape 1 : Mise à jour des chemins absolus

Mettre à jour les chemins absolu du fichier *startProcess.py* dans la liste **commandes** pour le lancement de LivePose et Ossia.

## Etape 2 : Modification des permissions du fichier de lancement Python

- Modifier les permissions du fichier Python *startProcess.py* pour le rendre exécutable : `chmod +x /chemin/vers/script/python/startProcess.py`

Remarque : Cette commande ne produit aucune sortie et vous retourne à l'invite de commande si celle-ci est réussie. 

- Vérifier si le fichier est maintenant exécutable : `ls -l /chemin/vers/script/python/startProcess.py`

Cette commande affichera les permissions du fichier. Voici un résultat de cette commande : **-rwxrwxr-x** 
Ce résultat signifie que le propriétaire du fichier peut lire, écrire et exécuter le fichier et que tous les autres utilisateurs peuvent lire et exécuter le fichier. 

## Etape 3 : Création du fichier *startProcess.desktop*

- Se rendre dans le dossier **config** : `cd ~/.config` 
- Entrer dans le dossier **autostart**. Le créer s'il n'existe pas : `mkdir ~/.config/autostart`
- Créer le fichier : `nano startProcess.desktop`

## Etape 4 : Configuration du fichier *startProcess.desktop*

Ajouter l'extrait de code suivant : 

```
[Desktop Entry]
Type=Application
Exec=python3 /chemin/vers/script/python/startProcess.py
Hidden=false
NoDisplay=false
X-GNOME-Autostart-enabled=true
Name[fr_FR]=Mon Script Python
Name=Mon Script Python
Comment[fr_FR]=Lancer mon script Python au démarrage
Comment=Lancer mon script Python au démarrage
```


- Mettre le chemin absolu du fichier de lancement startProcess.py dans le champ **Exec**
- Les champs "Name" et "Comment" peuvent être modifiés selon les préférences

## Etape 5 : Redémarrage du PC

- Redémarrer le pc et se connecter à l'utilisateur dans lequel les modifications ont été apportées.