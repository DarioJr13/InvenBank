import React, { useEffect, useState } from 'react';
import {
  IonPage, IonHeader, IonToolbar, IonButtons, IonBackButton, IonTitle,
  IonContent, IonCard, IonCardContent, IonItem, IonLabel, IonBadge, IonSelect,
  IonSelectOption, IonButton, IonIcon, IonSpinner, IonAlert, IonToast
} from '@ionic/react';
import { heart, heartOutline, cartOutline } from 'ionicons/icons';
import { useParams, useHistory } from 'react-router-dom';

import AddToCartModal from '../components/AddToCartModal';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { getProductDetailAsync, clearCurrentProduct } from '../store/productsSlice';
import { addToWishlistAsync, removeFromWishlistAsync } from '../store/wishlistSlice';
import { addToCart } from '../store/cartSlice';

interface ProductDetailParams {
  id: string;
}

const ProductDetail: React.FC = () => {
  const { id } = useParams<ProductDetailParams>();
  const history = useHistory();
  const dispatch = useAppDispatch();

  const { currentProduct, loading } = useAppSelector(state => state.products);
  const { items: wishlistItems } = useAppSelector(state => state.wishlist);

  const [selectedSupplier, setSelectedSupplier] = useState<number | null>(null);
  const [showCartModal, setShowCartModal] = useState(false);
  const [showToast, setShowToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');
  const [toastColor, setToastColor] = useState<'success' | 'danger'>('success');
  const [showPurchaseAlert, setShowPurchaseAlert] = useState(false);

  useEffect(() => {
    dispatch(getProductDetailAsync(parseInt(id)));
    return () => dispatch(clearCurrentProduct());
  }, [id, dispatch]);

  useEffect(() => {
    if (currentProduct?.suppliers?.length) {
      const cheapest = currentProduct.suppliers.reduce((prev, curr) =>
        prev.Price < curr.Price ? prev : curr
      );
      setSelectedSupplier(cheapest.Id);
    }
  }, [currentProduct]);

  const selectedSupplierData = currentProduct?.suppliers?.find(s => s.Id === selectedSupplier);
  const isInWishlist = wishlistItems.some(item => item.ProductId === parseInt(id));

  const formatPrice = (price: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP' }).format(price || 0);

  const toggleWishlist = async () => {
    try {
      if (isInWishlist) {
        await dispatch(removeFromWishlistAsync(parseInt(id))).unwrap();
        showNotification('Removido de favoritos', 'success');
      } else {
        await dispatch(addToWishlistAsync(parseInt(id))).unwrap();
        showNotification('Agregado a favoritos', 'success');
      }
    } catch (err) {
      showNotification('Error al actualizar favoritos', 'danger');
    }
  };

  const showNotification = (message: string, color: 'success' | 'danger') => {
    setToastMessage(message);
    setToastColor(color);
    setShowToast(true);
  };

  const handleAddToCartClick = () => {
    if (!selectedSupplierData) {
      showNotification('Selecciona un marca', 'danger');
      return;
    }
    setShowCartModal(true);
  };

  const confirmAddToCart = (quantity: number) => {
    if (!selectedSupplierData || !currentProduct) return;

    dispatch(
      addToCart({
        productId: currentProduct.product.Id,
        productName: currentProduct.product.Name,
        supplierId: selectedSupplier!,
        supplierName: selectedSupplierData.Name,
        price: selectedSupplierData.Price,
        quantity,
        total: quantity * selectedSupplierData.Price
      })
    );

    setShowCartModal(false);
    showNotification(`${quantity} ${currentProduct.product.Name} agregado al carrito`, 'success');
  };

  const getCartItem = () => {
    if (!selectedSupplierData || !currentProduct) return null;
    return {
      productId: currentProduct.product.Id,
      productName: currentProduct.product.Name,
      supplierId: selectedSupplier!,
      supplierName: selectedSupplierData.Name,
      price: selectedSupplierData.Price,
      maxStock: selectedSupplierData.Stock
    };
  };

  if (loading || !currentProduct) {
    return (
      <IonPage>
        <IonHeader>
          <IonToolbar>
            <IonButtons slot="start">
              <IonBackButton defaultHref="/products" />
            </IonButtons>
            <IonTitle>{loading ? 'Cargando...' : 'Producto no encontrado'}</IonTitle>
          </IonToolbar>
        </IonHeader>
        <IonContent>
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            {loading ? <IonSpinner /> : <p>No se pudo cargar el producto</p>}
            {!loading && <IonButton routerLink="/products">Volver</IonButton>}
          </div>
        </IonContent>
      </IonPage>
    );
  }

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonButtons slot="start">
            <IonBackButton defaultHref="/products" />
          </IonButtons>
          <IonTitle>Detalle de Producto</IonTitle>
          <IonButtons slot="end">
            <IonButton fill="clear" onClick={toggleWishlist}>
              <IonIcon icon={isInWishlist ? heart : heartOutline} color={isInWishlist ? 'danger' : 'medium'} />
            </IonButton>
          </IonButtons>
        </IonToolbar>
      </IonHeader>

      <IonContent>
        <IonCard>
          <IonCardContent>
            <h2>{currentProduct.product.Name}</h2>
            <p style={{ color: 'gray' }}>{currentProduct.product.Description}</p>
            <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
              <IonBadge color="primary">{currentProduct.product.Category}</IonBadge>
              <IonBadge color="secondary">{currentProduct.suppliers?.length} Marca</IonBadge>
            </div>
          </IonCardContent>
        </IonCard>

        <IonCard>
          <IonCardContent>
            <h3>Selecciona una Marca</h3>
            <IonItem>
              <IonLabel>Marca</IonLabel>
              <IonSelect value={selectedSupplier} onIonChange={e => setSelectedSupplier(e.detail.value)}>
                {currentProduct.suppliers?.map(s => (
                  <IonSelectOption key={s.Id} value={s.Id}>
                    {s.Name} - {formatPrice(s.Price)}
                  </IonSelectOption>
                ))}
              </IonSelect>
            </IonItem>

            {selectedSupplierData && (
              <div style={{ marginTop: '1rem', backgroundColor: '#f8f8f8', padding: '1rem', borderRadius: '8px' }}>
                <strong>{selectedSupplierData.Name}</strong>
                <IonBadge color="success" style={{ marginLeft: '0.5rem' }}>Disponible</IonBadge>
                <div style={{ fontSize: '1.1rem', marginTop: '0.5rem' }}>
                  {formatPrice(selectedSupplierData.Price)} / Stock: {selectedSupplierData.Stock}
                </div>
              </div>
            )}

            <IonButton
              expand="block"
              onClick={handleAddToCartClick}
              disabled={!selectedSupplierData || selectedSupplierData.Stock === 0}
              style={{ marginTop: '1rem' }}
            >
              <IonIcon icon={cartOutline} slot="start" />
              Añadir al Carrito
            </IonButton>
          </IonCardContent>
        </IonCard>

        <IonAlert
          isOpen={showPurchaseAlert}
          onDidDismiss={() => setShowPurchaseAlert(false)}
          header="Compra Simulada"
          message={`¿Confirmar compra de "${currentProduct.product.Name}"?`}
          buttons={[
            { text: 'Cancelar', role: 'cancel' },
            {
              text: 'Confirmar',
              handler: () => console.log('Compra simulada')
            }
          ]}
        />

        <AddToCartModal
          isOpen={showCartModal}
          onDidDismiss={() => setShowCartModal(false)}
          item={getCartItem()}
          onConfirm={confirmAddToCart}
        />

        <IonToast
          isOpen={showToast}
          message={toastMessage}
          duration={2000}
          color={toastColor}
          onDidDismiss={() => setShowToast(false)}
        />
      </IonContent>
    </IonPage>
  );
};

export default ProductDetail;
