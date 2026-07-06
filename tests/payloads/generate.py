import json
import random
from pathlib import Path

print("Starting JSON payload generation for JMeter...\n")

OUTPUT_FILE = 'reviews_bulk_large.json'
ARGS_COUNT = 500

script_dir = Path(__file__).resolve().parent
payloads_dir = Path(__file__).resolve().parent
output_file = payloads_dir / OUTPUT_FILE
payloads_dir.mkdir(parents=True, exist_ok=True)

FIRST_NAMES = ["Adam", "Ewa", "Michal", "Karolina", "Piotr", "Katarzyna", "Tomasz", "Magdalena", "Grzegorz"]
LAST_NAMES = ["Nowak", "Kowalski", "Wisniewski", "Wojcik", "Kowalczyk", "Kaminski", "Lewandowski"]
DOMAINS = ["gmail.com", "wp.pl", "onet.pl", "interia.pl", "example.com"]

VALID_PRODUCT_IDS = [1, 2, 3, 4, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 326, 327, 328, 329, 330]

payload = []

for _ in range(ARGS_COUNT):
    fname = random.choice(FIRST_NAMES)
    lname = random.choice(LAST_NAMES)
    
    review = {
        "productId": random.choice(VALID_PRODUCT_IDS),
        "reviewerName": f"{fname} {lname}",
        "emailAddress": f"{fname.lower()}.{lname.lower()}@{random.choice(DOMAINS)}",
        "rating": random.randint(1, 5),
        "comments": "JMeterLoadTest"
    }
    payload.append(review)

with open(output_file, "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)

print(f"[SUCCESS] Generated {ARGS_COUNT} reviews.")
print(f"[INFO] Saved payload to: {output_file.resolve()}")