# Build Phase
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build --prod

# Run Phase
FROM nginx:stable
COPY --from=build /app/dist/* /usr/share/nginx/html
EXPOSE 8080

# Make Nginx listen on 8080 instead of 80
RUN sed -i 's/listen 80;/listen 8080;/g' /etc/nginx/conf.d/default.conf

CMD ["nginx", "-g", "daemon off;"]
