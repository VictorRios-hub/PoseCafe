import subprocess
import time


#Command lines to run


commandes = [
    # START LIVEPOSE
    'cd /home/metalab_legion/Git/livepose; ./livepose.sh -f -c livepose/configs/test_generic_2d_to_3d.json',
    # START OSSIA
    'ossia.score-3.1.10-linux-amd64.AppImage --autoplay /home/victor/Desktop/2023-stagiaires-posecafe/Ossia_config/PoseCafe_score.score'
]

delay = 5

for commande in commandes:

    print(f"Command line running: {commande}")

    process = subprocess.Popen(['gnome-terminal', '--', 'bash', '-c', f'{commande}; exec bash'])

    time.sleep(delay)