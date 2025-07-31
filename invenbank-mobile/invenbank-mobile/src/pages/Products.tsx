import React, { useEffect, useState } from 'react';
import {
  IonContent,
  IonHeader,
  IonPage,
  IonTitle,
  IonToolbar,
  IonSearchbar,
  IonCard,
  IonCardContent,
  IonItem,
  IonLabel,
  IonSpinner,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonButton,
  IonIcon,
  IonBadge,
  IonRefresher,
  IonRefresherContent
} from '@ionic/react';
import { heart, heartOutline, pricetagOutline } from 'ionicons/icons';
import { useHistory } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { searchProductsAsync, setSearchTerm } from '../store/productsSlice';
import { addToWishlistAsync, removeFromWishlistAsync } from '../store/wishlistSlice';

const Products: React.FC = () => {
  const [searchText, setSearchText] = useState('');
  const dispatch = useAppDispatch();
  const history = useHistory();
  
  const { products, loading, searchTerm, pagination } = useAppSelector(state => state.products);
  const { items: wishlistItems } = useAppSelector(state => state.wishlist);
  const { isAuthenticated } = useAppSelector(state => state.auth);

  useEffect(() => {
    if (!isAuthenticated) {
      history.replace('/login');
      return;
    }
    
    // Cargar productos iniciales
    dispatch(searchProductsAsync({ pageNumber: 1, pageSize: 20 }));
  }, [isAuthenticated, dispatch, history]);

  const handleSearch = (e: CustomEvent) => {
    const term = e.detail.value!;
    setSearchText(term);
    dispatch(setSearchTerm(term));
    
    // Buscar con delay para evitar muchas peticiones
    setTimeout(() => {
      dispatch(searchProductsAsync({ 
        searchTerm: term,
        pageNumber: 1,
        pageSize: 20 
      }));
    }, 500);
  };

  const loadMore = async (e: CustomEvent) => {
    if (pagination.hasNextPage) {
      await dispatch(searchProductsAsync({
        searchTerm,
        pageNumber: pagination.pageNumber + 1,
        pageSize: 20
      }));
    }
    (e.target as HTMLIonInfiniteScrollElement).complete();
  };

  const handleRefresh = async (e: CustomEvent) => {
    await dispatch(searchProductsAsync({ 
      searchTerm,
      pageNumber: 1,
      pageSize: 20 
    }));
    (e.target as HTMLIonRefresherElement).complete();
  };

  const toggleWishlist = (productId: number) => {
    const isInWishlist = wishlistItems.some(item => item.productId === productId);
    
    if (isInWishlist) {
      dispatch(removeFromWishlistAsync(productId));
    } else {
      dispatch(addToWishlistAsync(productId));
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP'
    }).format(price);
  };

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Productos</IonTitle>
        </IonToolbar>
      </IonHeader>
      
      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={handleRefresh}>
          <IonRefresherContent />
        </IonRefresher>

        <IonSearchbar
          value={searchText}
          onIonInput={handleSearch}
          placeholder="Buscar productos..."
          debounce={300}
        />

        {loading && !products.length && (
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <IonSpinner />
            <p>Cargando productos...</p>
          </div>
        )}

        {products.map((product) => {
          const isInWishlist = wishlistItems.some(item => item.productId === product.id);
          
          return (
            <IonCard key={product.id} button onClick={() => history.push(`/products/${product.id}`)}>
              <IonCardContent>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div style={{ flex: 1 }}>
                    <h3 style={{ margin: '0 0 0.5rem 0' }}>{product.name}</h3>
                    <p style={{ color: 'gray', fontSize: '0.9rem', margin: '0 0 0.5rem 0' }}>
                      {product.description}
                    </p>
                    
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                      <IonIcon icon={pricetagOutline} />
                      <span style={{ fontWeight: 'bold', color: 'var(--ion-color-primary)' }}>
                        {formatPrice(product.minPrice)}
                        {product.minPrice !== product.maxPrice && (
                          <span style={{ fontWeight: 'normal', color: 'gray' }}>
                            {' - ' + formatPrice(product.maxPrice)}
                          </span>
                        )}
                      </span>
                    </div>

                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      <IonBadge color={product.isAvailable ? 'success' : 'danger'}>
                        Stock: {product.totalStock}
                      </IonBadge>
                      <IonBadge color="medium">
                        {product.supplierCount} proveedor{product.supplierCount !== 1 ? 'es' : ''}
                      </IonBadge>
                    </div>
                  </div>

                  <IonButton
                    fill="clear"
                    size="small"
                    onClick={(e) => {
                      e.stopPropagation();
                      toggleWishlist(product.id);
                    }}
                  >
                    <IonIcon 
                      icon={isInWishlist ? heart : heartOutline} 
                      color={isInWishlist ? 'danger' : 'medium'} 
                    />
                  </IonButton>
                </div>
              </IonCardContent>
            </IonCard>
          );
        })}

        <IonInfiniteScroll onIonInfinite={loadMore} disabled={!pagination.hasNextPage}>
          <IonInfiniteScrollContent loadingText="Cargando más productos..." />
        </IonInfiniteScroll>
      </IonContent>
    </IonPage>
  );
};

export default Products;