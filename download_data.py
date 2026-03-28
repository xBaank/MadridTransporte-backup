import os
import requests

GTFS_FILES = [
    ("google_transit_M9", "https://www.arcgis.com/sharing/rest/content/items/357e63c2904f43aeb5d8a267a64346d8/data"),
    ("google_transit_M89", "https://www.arcgis.com/sharing/rest/content/items/885399f83408473c8d815e40c5e702b7/data"),
    ("google_transit_M4", "https://www.arcgis.com/sharing/rest/content/items/5c7f2951962540d69ffe8f640d94c246/data"),
    ("google_transit_M6", "https://www.arcgis.com/sharing/rest/content/items/868df0e58fca47e79b942902dffd7da0/data"),
    ("google_transit_M10", "https://www.arcgis.com/sharing/rest/content/items/aaed26cc0ff64b0c947ac0bc3e033196/data"),
    ("google_transit_M5", "https://www.arcgis.com/sharing/rest/content/items/1a25440bf66f499bae2657ec7fb40144/data"),
]

OTHER_FILES = [
    ("Metro_stations", "https://hub.arcgis.com/api/download/v1/items/0a6c45e7bdd94679b67a2ae662c8838b/csv?redirect=true&layers=0"),
    ("Train_stations", "https://hub.arcgis.com/api/download/v1/items/9e353bbf4c5d4bea87f01d6d579d06ab/csv?redirect=true&layers=0"),
    ("Train_itineraries", "https://hub.arcgis.com/api/download/v1/items/9e353bbf4c5d4bea87f01d6d579d06ab/csv?redirect=true&layers=5"),
]

TRAM_NAME = "Tram_stations"
TRAM_URL = "https://hub.arcgis.com/api/download/v1/items/53c45916691a4256bf0f6f69fb0e182c/csv?redirect=true&layers=0"


def download_file(url, dest_path):
    response = requests.get(url, allow_redirects=True)
    response.raise_for_status()
    with open(dest_path, "wb") as f:
        f.write(response.content)
    print(f"Downloaded: {dest_path}")


def main():
    for name, url in GTFS_FILES:
        download_file(url, f"{name}.zip")

    for name, url in OTHER_FILES:
        download_file(url, f"{name}.csv")

    download_file(TRAM_URL, f"{TRAM_NAME}.csv")


if __name__ == "__main__":
    main()
