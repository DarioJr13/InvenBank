import React, { useEffect, useState } from 'react';
import {
  IonContent,
  IonHeader,
  IonPage,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonCard,
  IonCardContent,
  IonItem,
  IonLabel,
  IonButton,
  IonIcon,
  IonBadge,
  IonSpinner,
  IonAlert,
  IonSelect,
  IonSelectOption
} from '@ionic/react';
import { heart, heartOutline, cartOutline, chevronBack } from 'ionicons/icons';
import { useParams, useHistory } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { getProductDetailAsync, clearCurrentProduct } from '../store/productsSlice';
import { addToWishlistAsync, removeFromWishlistAsync } from '../store/wishlistSlice';

interface ProductDetailParams {
  id: string;
}

const ProductDetail: React.FC = () => {
  const { id } = useParams<ProductDetailParams>();
  const [selectedSupplier, setSelectedSupplier] = useState<number | null>(null);
  const [showPurchaseAlert, setShowPurchaseAlert] = useState(false);
  
  const dispatch = useAppDispatch();
  const history = useHistory();
  
  const { currentProduct, loading } = useAppSelector(state => state.products);
  const { items: wishlistItems } = useAppSelector(state => state.wishlist);

  useEffect(() => {
    dispatch(getProductDetailAsync(parseInt(id)));
    
    return () => {
      dispatch(clearCurrentProduct());
    };
  }, [id, dispatch]);

  useEffect(() => {
    if (currentProduct?.suppliers?.length) {
      // Seleccionar el proveedor preferido o el más barato por defecto
      const preferred = currentProduct.suppliers.find(s => s.isPreferred);
      const cheapest = currentProduct.suppliers.reduce((prev, curr) => 
        prev.price < curr.price ? prev : curr
      );
      setSelectedSupplier((preferred || cheapest).id);
    }
  }, [currentProduct]);

  const toggleWishlist = () => {
    if (!currentProduct) return;
    
    const isInWishlist = wishlistItems.some(item => item.productId === currentProduct.id);
    
    if (isInWishlist) {
      dispatch(removeFromWishlistAsync(currentProduct.id));
    } else {
      dispatch(addToWishlistAsync(currentProduct.id));
    }
  };

  const handlePurchase = () => {
    setShowPurchaseAlert(true);
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP'
    }).format(price);
  };

  if (loading || !currentProduct) {
    return (
      <IonPage>
        <IonHeader>
          <IonToolbar>
            <IonButtons slot="start">
              <IonBackButton defaultHref="/products" />
            </IonButtons>
            <IonTitle>Cargando...</IonTitle>
          </IonToolbar>
        </IonHeader>
        <IonContent>
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <IonSpinner />
          </div>
        </IonContent>
      </IonPage>
    );
  }

  const isInWishlist = wishlistItems.some(item => item.productId === currentProduct.id);
  const selectedSupplierData = currentProduct.suppliers.find(s => s.id === selectedSupplier);

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonButtons slot="start">
            <IonBackButton defaultHref="/products" />
          </IonButtons>
          <IonTitle>{currentProduct.name}</IonTitle>
          <IonButtons slot="end">
            <IonButton onClick={toggleWishlist}>
              <IonIcon 
                icon={isInWishlist ? heart : heartOutline} 
                color={isInWishlist ? 'danger' : 'medium'} 
              />
            </IonButton>
          </IonButtons>
        </IonToolbar>
      </IonHeader>
      
      <IonContent>
        <IonCard>
          <IonCardContent>
            <h1>{currentProduct.name}</h1>
            <p style={{ color: 'gray' }}>{currentProduct.description}</p>
            
            <div style={{ marginBottom: '1rem' }}>
              <IonLabel>
                <strong>SKU:</strong> {currentProduct.sku}
              </IonLabel>
              <br />
              <IonLabel>
                <strong>Categoría:</strong> {currentProduct.categoryName}
              </IonLabel>
            </div>

            <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
              <IonBadge color={currentProduct.isAvailable ? 'success' : 'danger'}>
                Stock Total: {currentProduct.totalStock}
              </IonBadge>
              <IonBadge color="primary">
                {currentProduct.supplierCount} Proveedores
              </IonBadge>
            </div>
          </IonCardContent>
        </IonCard>

        <IonCard>
          <IonCardContent>
            <h3>Seleccionar Proveedor</h3>
            
            <IonItem>
              <IonLabel>Proveedor</IonLabel>
              <IonSelect
                value={selectedSupplier}
                onSelectionChange={(e) => setSelectedSupplier(e.detail.value)}
              >
                {currentProduct.suppliers.map((supplier) => (
                  <IonSelectOption key={supplier.id} value={supplier.id}>
                    {supplier.supplierName} - {formatPrice(supplier.price)}
                    {supplier.isPreferred && ' (Recomendado)'}
                  </IonSelectOption>
                ))}
              </IonSelect>
            </IonItem>

            {selectedSupplierData && (
              <div style={{ marginTop: '1rem', padding: '1rem', backgroundColor: 'var(--ion-color-light)', borderRadius: '8px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div>
                    <strong>{selectedSupplierData.supplierName}</strong>
                    {selectedSupplierData.isPreferred && (
                      <IonBadge color="warning" style={{ marginLeft: '0.5rem' }}>
                        Recomendado
                      </IonBadge>
                    )}
                  </div>
                  <div style={{ textAlign: 'right' }}>
                    <div style={{ fontSize: '1.2rem', fontWeight: 'bold', color: 'var(--ion-color-primary)' }}>
                      {formatPrice(selectedSupplierData.price)}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: 'gray' }}>
                      Stock: {selectedSupplierData.stock}
                    </div>
                  </div>
                </div>
              </div>
            )}

            <IonButton
              expand="block"
              onClick={handlePurchase}
              disabled={!selectedSupplierData || selectedSupplierData.stock === 0}
              style={{ marginTop: '1rem' }}
            >
              <IonIcon icon={cartOutline} slot="start" />
              Comprar Ahora
            </IonButton>
          </IonCardContent>
        </IonCard>

        <IonAlert
          isOpen={showPurchaseAlert}
          onDidDismiss={() => setShowPurchaseAlert(false)}
          header="Compra Simulada"
          message={`¿Confirmar compra de "${currentProduct.name}" por ${selectedSupplierData ? formatPrice(selectedSupplierData.price) : ''}?`}
          buttons={[
            {
              text: 'Cancelar',
              role: 'cancel'
            },
            {
              text: 'Confirmar',
              handler: () => {
                // Aquí implementarías la lógica de compra real
                console.log('Compra confirmada:', {
                  productId: currentProduct.id,
                  supplierId: selectedSupplier,
                  price: selectedSupplierData?.price
                });
              }
            }
          ]}
        />
      </IonContent>
    </IonPage>
  );
};

export default ProductDetail;