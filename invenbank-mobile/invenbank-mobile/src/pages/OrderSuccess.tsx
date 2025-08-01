import React, { useEffect } from 'react';
import {
  IonPage, IonHeader, IonToolbar, IonTitle,
  IonContent, IonButton
} from '@ionic/react';
import Lottie from 'lottie-react';
import successAnimation from '../assets/lottie/success.json';
import confetti from 'canvas-confetti';

const OrderSuccess: React.FC = () => {
  useEffect(() => {
    // 🎉 Disparo de confeti
    confetti({
      particleCount: 150,
      spread: 100,
      origin: { y: 0.6 },
    });
  }, []);

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>¡Orden Exitosa!</IonTitle>
        </IonToolbar>
      </IonHeader>
      <IonContent className="ion-padding" style={{ textAlign: 'center' }}>
        <Lottie
          animationData={successAnimation}
          loop={false}
          style={{ height: 250, width: 250, margin: '2rem auto' }}
        />

        <h2>¡Gracias por tu compra!</h2>
        <p>Tu orden fue procesada correctamente.</p>

        <IonButton routerLink="/products" expand="block" color="primary" style={{ marginTop: '2rem' }}>
          Volver a Productos
        </IonButton>
      </IonContent>
    </IonPage>
  );
};

export default OrderSuccess;
