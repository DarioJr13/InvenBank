import React, { useState } from 'react';
import {
  IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
  IonCard, IonCardContent, IonItem, IonLabel, IonButton, IonIcon, IonToast, IonAlert
} from '@ionic/react';
import { trash } from 'ionicons/icons';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { removeFromCart, clearCart } from '../store/cartSlice';
import { orderService } from '../services/orderService';
import { useHistory } from 'react-router-dom';

const Cart: React.FC = () => {
  const dispatch = useAppDispatch();
  const history = useHistory();
  const { items } = useAppSelector(state => state.cart);

  const [showToast, setShowToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');
  const [showConfirm, setShowConfirm] = useState(false);

  const total = items.reduce((sum, item) => sum + item.total, 0);

  const formatPrice = (price: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP' }).format(price || 0);

  const handleRemove = (productId: number, supplierId: number) => {
    dispatch(removeFromCart({ productId, supplierId }));
    setToastMessage('Producto eliminado del carrito');
    setShowToast(true);
  };

const handleConfirmPurchase = async () => {
  try {
    await orderService.createOrder({
      items: items.map(item => ({
        productId: item.productId,
        supplierId: item.supplierId,
        quantity: item.quantity,
        unitPrice: item.price
      })),
      notes: 'Compra realizada desde la app móvil'
    });

    dispatch(clearCart());
    setTimeout(() => history.replace('/orden-exitosa'), 1000);
  } catch (err) {
    console.error('❌ Error al crear orden:', err);
    setToastMessage('Error al procesar la compra');
    setShowToast(true);
  }
};



  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Resumen del Carrito</IonTitle>
        </IonToolbar>
      </IonHeader>
      <IonContent>
        {items.map((item, index) => (
          <IonCard key={index}>
            <IonCardContent>
              <IonItem>
                <IonLabel>
                  <h3>{item.productName}</h3>
                  <p>Proveedor: {item.supplierName}</p>
                  <p>Cantidad: {item.quantity}</p>
                  <p>Precio unitario: {formatPrice(item.price)}</p>
                  <p>Total: {formatPrice(item.total)}</p>
                </IonLabel>
                <IonButton
                  fill="clear"
                  color="danger"
                  onClick={() => handleRemove(item.productId, item.supplierId)}
                >
                  <IonIcon icon={trash} />
                </IonButton>
              </IonItem>
            </IonCardContent>
          </IonCard>
        ))}

        {items.length > 0 && (
          <div style={{ padding: '1rem' }}>
            <h2>Total: {formatPrice(total)}</h2>
            <IonButton expand="block" color="success" onClick={() => setShowConfirm(true)}>
              Páguelo ISI
            </IonButton>
          </div>
        )}

        {items.length === 0 && (
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <p>Tu carrito está vacío</p>
            <IonButton routerLink="/products">Ir a productos</IonButton>
          </div>
        )}

        <IonAlert
          isOpen={showConfirm}
          onDidDismiss={() => setShowConfirm(false)}
          header="¿Confirmar compra?"
          message={`Total: ${formatPrice(total)}\n¿Deseas proceder con la compra?`}
          buttons={[
            { text: 'Cancelar', role: 'cancel' },
            { text: 'Confirmar', handler: handleConfirmPurchase }
          ]}
        />

        <IonToast
          isOpen={showToast}
          message={toastMessage}
          duration={2000}
          color="primary"
          onDidDismiss={() => setShowToast(false)}
        />
      </IonContent>
    </IonPage>
  );
};

export default Cart;
