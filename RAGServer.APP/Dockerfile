# --- Build Phase ---
FROM node:20 AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# Angular output confirmed as:
# dist/RAGServerAPP/browser

# --- Run Phase ---
FROM nginx:stable

ENV PORT=8080

# Copy the correct Angular dist path
COPY --from=build /app/dist/RAGServerAPP/browser /usr/share/nginx/html

# Update Nginx to listen on Cloud Run port
RUN sed -i "s/listen       80;/listen       ${PORT};/g" /etc/nginx/conf.d/default.conf

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]
