"use strict";

// Asegúrate de incluir signalr.js antes de este script
// <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.js"></script>

class ChatClient {
    constructor(hubUrl, accessToken) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => accessToken
            })
            .withAutomaticReconnect()
            .build();

        this.onReceiveMessage = null;
        this.onMessageSent = null;
    }

    async start() {
        try {
            await this.connection.start();
            console.log("SignalR Connected.");
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
            setTimeout(() => this.start(), 5000);
        }
    }

    async sendMessage(receiverId, message, bookingId) {
        try {
            // senderId se infiere del token en el servidor, pero el Hub espera senderId como arg?
            // Revisemos ChatHub.SendMessage(string senderId, ...)
            // Sí, el Hub espera senderId. Deberíamos obtenerlo del usuario actual.
            // Para simplificar, pasaremos el ID del usuario actual que debemos tener en el frontend.
            var senderId = document.getElementById('currentUserId').value;
            await this.connection.invoke("SendMessage", senderId, receiverId, message, bookingId);
        } catch (err) {
            console.error(err);
        }
    }

    registerHandlers() {
        this.connection.on("ReceiveMessage", (senderId, message, bookingId, timestamp) => {
            if (this.onReceiveMessage) this.onReceiveMessage(senderId, message, bookingId, timestamp);
        });

        this.connection.on("MessageSent", (chatMessage) => {
            if (this.onMessageSent) this.onMessageSent(chatMessage);
        });
    }
}
