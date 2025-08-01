import React, { useState, useEffect } from 'react';
import {
  IonModal,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonButtons,
  IonButton,
  IonContent,
  IonCard,
  IonCardContent,
  IonItem,
  IonLabel,
  IonInput,
  IonText
} from '@ionic/react';

interface CartItem {
  productId: number;
  productName: string;
  supplierId: number;
  supplierName: string;
  price: number;
  maxStock: number;
}

interface AddToCartModalProps {
  isOpen: boolean;
  onDidDismiss: () => void;
  item: CartItem | null;
  onConfirm: (quantity: number) => void;
}

const AddToCartModal: React.FC<AddToCartModalProps> = ({
  isOpen,
  onDidDismiss,
  item,
  onConfirm
}) => {
  const [quantity, setQuantity] = useState(1);
  const [total, setTotal] = useState(0);

  useEffect(() => {
    if (isOpen && item) {
      setQuantity(1);
      setTotal(item.price);
    }
  }, [isOpen, item]);

  useEffect(() => {
    if (item) {
      setTotal(quantity * item.price);
    }
  }, [quantity, item]);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP'
    }).format(price);
  };

  const handleConfirm = () => {
    if (item && quantity > 0 && quantity <= item.maxStock) {
      onConfirm(quantity);
    }
  };

  const handleQuantityChange = (value: string) => {
    const newQuantity = Math.min(Math.max(parseInt(value || '1'), 1), item?.maxStock || 1);
    setQuantity(newQuantity);
  };

  if (!item) return null;

  const isInvalid = quantity <= 0 || quantity > item.maxStock;

  return (
    <IonModal isOpen={isOpen} onDidDismiss={onDidDismiss}>
      <IonHeader>
        <IonToolbar>
          <IonTitle>
            {item.productName} - {formatPrice(total)}
          </IonTitle>
          <IonButtons slot="end">
            <IonButton onClick={onDidDismiss}>Cerrar</IonButton>
          </IonButtons>
        </IonToolbar>
      </IonHeader>
      <IonContent>
        <div style={{ padding: '1rem' }}>
          <IonCard>
            <IonCardContent>
              <h3>{item.productName}</h3>
              <p>Proveedor: {item.supplierName}</p>
              <p>Precio unitario: {formatPrice(item.price)}</p>
              <p>Stock disponible: {item.maxStock}</p>
            </IonCardContent>
          </IonCard>

          <IonItem>
            <IonLabel position="stacked">Cantidad</IonLabel>
            <IonInput
              type="number"
              value={quantity}
              min={1}
              max={item.maxStock}
              onIonInput={(e) => handleQuantityChange(e.detail.value!)}
            />
            <IonText color={isInvalid ? 'danger' : 'medium'}>
              Máximo: {item.maxStock}
            </IonText>
          </IonItem>

          <div style={{ padding: '1rem 0', textAlign: 'center' }}>
            <h4>Total: {formatPrice(total)}</h4>
          </div>

          <IonButton
            expand="block"
            onClick={handleConfirm}
            disabled={isInvalid}
            style={{ marginTop: '1rem' }}
          >
            Agregar {quantity} {quantity === 1 ? 'producto' : 'productos'} - {formatPrice(total)}
          </IonButton>
        </div>
      </IonContent>
    </IonModal>
  );
};

export default AddToCartModal;
