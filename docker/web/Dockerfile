FROM python:3.9-alpine

WORKDIR /usr/src/app

ENV DB_IP="dbserver"

COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

CMD [ "python", "./minitwit.py" ]
