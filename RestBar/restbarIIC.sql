--
-- PostgreSQL database dump
--

-- Dumped from database version 17.2
-- Dumped by pg_dump version 17.2

-- Started on 2025-08-05 17:35:37

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 6 (class 2615 OID 175235)
-- Name: user_role_enum; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA user_role_enum;


ALTER SCHEMA user_role_enum OWNER TO postgres;

--
-- TOC entry 995 (class 1247 OID 175976)
-- Name: movement_type_enum; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.movement_type_enum AS ENUM (
    'purchase',
    'sale',
    'adjustment',
    'transfer',
    'return',
    'waste',
    'initial',
    'correction'
);


ALTER TYPE public.movement_type_enum OWNER TO postgres;

--
-- TOC entry 890 (class 1247 OID 175215)
-- Name: user_role_enum; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.user_role_enum AS ENUM (
    'accountant',
    'admin',
    'bartender',
    'cashier',
    'chef',
    'inventory',
    'manager',
    'supervisor',
    'support',
    'waiter'
);


ALTER TYPE public.user_role_enum OWNER TO postgres;

--
-- TOC entry 893 (class 1247 OID 175237)
-- Name: user_role; Type: TYPE; Schema: user_role_enum; Owner: postgres
--

CREATE TYPE user_role_enum.user_role AS ENUM (
    'admin',
    'manager',
    'supervisor',
    'waiter',
    'cashier',
    'chef',
    'bartender',
    'inventory',
    'accountant',
    'support'
);


ALTER TYPE user_role_enum.user_role OWNER TO postgres;

--
-- TOC entry 258 (class 1255 OID 176244)
-- Name: update_companies_updated_at(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_companies_updated_at() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW."UpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.update_companies_updated_at() OWNER TO postgres;

--
-- TOC entry 257 (class 1255 OID 176160)
-- Name: update_updated_at_column(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_updated_at_column() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW."UpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.update_updated_at_column() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 254 (class 1259 OID 176045)
-- Name: PurchaseOrderItems; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PurchaseOrderItems" (
    "Id" uuid NOT NULL,
    "PurchaseOrderId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "Quantity" integer NOT NULL,
    "Subtotal" numeric(18,2) NOT NULL,
    "TaxRate" numeric(18,2) NOT NULL,
    "TaxAmount" numeric(18,2) NOT NULL,
    "TotalAmount" numeric(18,2) NOT NULL,
    "ReceivedQuantity" integer,
    "Notes" character varying(200),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public."PurchaseOrderItems" OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 176038)
-- Name: PurchaseOrders; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PurchaseOrders" (
    "Id" uuid NOT NULL,
    "OrderNumber" character varying(50) NOT NULL,
    "SupplierId" uuid NOT NULL,
    "CompanyId" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "CreatedById" uuid NOT NULL,
    "OrderDate" timestamp with time zone NOT NULL,
    "ExpectedDeliveryDate" timestamp with time zone,
    "ActualDeliveryDate" timestamp with time zone,
    "Subtotal" numeric(18,2) NOT NULL,
    "TaxAmount" numeric(18,2) NOT NULL,
    "TotalAmount" numeric(18,2) NOT NULL,
    "Notes" character varying(500),
    "Status" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public."PurchaseOrders" OWNER TO postgres;

--
-- TOC entry 256 (class 1259 OID 176088)
-- Name: TransferItems; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TransferItems" (
    "Id" uuid NOT NULL,
    "TransferId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "Quantity" integer NOT NULL,
    "Subtotal" numeric(18,2) NOT NULL,
    "TaxRate" numeric(18,2) NOT NULL,
    "TaxAmount" numeric(18,2) NOT NULL,
    "TotalAmount" numeric(18,2) NOT NULL,
    "ReceivedQuantity" integer,
    "Notes" character varying(200),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public."TransferItems" OWNER TO postgres;

--
-- TOC entry 255 (class 1259 OID 176081)
-- Name: Transfers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Transfers" (
    "Id" uuid NOT NULL,
    "TransferNumber" character varying(50) NOT NULL,
    "SourceBranchId" uuid NOT NULL,
    "DestinationBranchId" uuid NOT NULL,
    "CompanyId" uuid NOT NULL,
    "CreatedById" uuid NOT NULL,
    "ApprovedById" uuid,
    "ReceivedById" uuid,
    "TransferDate" timestamp with time zone NOT NULL,
    "ExpectedDeliveryDate" timestamp with time zone,
    "ActualDeliveryDate" timestamp with time zone,
    "Subtotal" numeric(18,2) NOT NULL,
    "TaxAmount" numeric(18,2) NOT NULL,
    "TotalAmount" numeric(18,2) NOT NULL,
    "Notes" character varying(500),
    "Status" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public."Transfers" OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 175209)
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 175257)
-- Name: accounts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.accounts (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    code character varying(20) NOT NULL,
    name character varying(200) NOT NULL,
    description character varying(500),
    type integer NOT NULL,
    category integer NOT NULL,
    nature integer NOT NULL,
    parent_account_id uuid,
    is_active boolean DEFAULT true NOT NULL,
    is_system boolean DEFAULT false NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone,
    created_by character varying(100),
    updated_by character varying(100),
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.accounts OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 175373)
-- Name: areas; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.areas (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    branch_id uuid,
    name character varying(50) NOT NULL,
    "Description" text,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.areas OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 175431)
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.audit_logs (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid,
    action text NOT NULL,
    table_name character varying(100),
    record_id uuid,
    "timestamp" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    company_id uuid,
    branch_id uuid,
    old_values text,
    new_values text,
    ip_address text,
    user_agent text,
    "CompanyId" uuid,
    "BranchId" uuid,
    "LogLevel" character varying(50),
    "Module" character varying(100),
    "Description" character varying(500),
    "OldValues" text,
    "NewValues" text,
    "ErrorDetails" text,
    "IpAddress" character varying(200),
    "UserAgent" character varying(500),
    "SessionId" character varying(100),
    "IsError" boolean DEFAULT false,
    "ErrorCode" integer,
    "ExceptionType" character varying(200),
    "StackTrace" text,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.audit_logs OWNER TO postgres;

--
-- TOC entry 5485 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."CompanyId"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."CompanyId" IS 'ID de la compañía para multi-tenant';


--
-- TOC entry 5486 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."BranchId"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."BranchId" IS 'ID de la sucursal para multi-tenant';


--
-- TOC entry 5487 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."LogLevel"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."LogLevel" IS 'Nivel de logging: INFO, WARNING, ERROR, CRITICAL';


--
-- TOC entry 5488 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."Module"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."Module" IS 'Módulo del sistema: User, Order, Inventory, etc.';


--
-- TOC entry 5489 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."Description"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."Description" IS 'Descripción detallada de la acción';


--
-- TOC entry 5490 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."OldValues"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."OldValues" IS 'Valores anteriores en formato JSON';


--
-- TOC entry 5491 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."NewValues"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."NewValues" IS 'Valores nuevos en formato JSON';


--
-- TOC entry 5492 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."ErrorDetails"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."ErrorDetails" IS 'Detalles del error en formato JSON';


--
-- TOC entry 5493 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."IpAddress"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."IpAddress" IS 'Dirección IP del usuario';


--
-- TOC entry 5494 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."UserAgent"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."UserAgent" IS 'User Agent del navegador';


--
-- TOC entry 5495 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."SessionId"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."SessionId" IS 'ID de la sesión del usuario';


--
-- TOC entry 5496 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."IsError"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."IsError" IS 'Indica si es un log de error';


--
-- TOC entry 5497 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."ErrorCode"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."ErrorCode" IS 'Código de error numérico';


--
-- TOC entry 5498 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."ExceptionType"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."ExceptionType" IS 'Tipo de excepción';


--
-- TOC entry 5499 (class 0 OID 0)
-- Dependencies: 232
-- Name: COLUMN audit_logs."StackTrace"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.audit_logs."StackTrace" IS 'Stack trace completo del error';


--
-- TOC entry 224 (class 1259 OID 175307)
-- Name: branches; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.branches (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    company_id uuid,
    name character varying(100) NOT NULL,
    address text,
    phone character varying(20),
    is_active boolean DEFAULT true,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.branches OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 175322)
-- Name: categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.categories (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(500),
    is_active boolean DEFAULT true NOT NULL,
    "CompanyId" uuid,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.categories OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 175273)
-- Name: companies; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.companies (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(100) NOT NULL,
    tax_id text,
    address text,
    phone text,
    email text,
    is_active boolean DEFAULT true NOT NULL,
    legal_id character varying(50),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    created_by character varying(255),
    updated_by character varying(255)
);


ALTER TABLE public.companies OWNER TO postgres;

--
-- TOC entry 5500 (class 0 OID 0)
-- Dependencies: 220
-- Name: COLUMN companies.tax_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.companies.tax_id IS 'Número de identificación fiscal';


--
-- TOC entry 5501 (class 0 OID 0)
-- Dependencies: 220
-- Name: COLUMN companies.address; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.companies.address IS 'Dirección de la compañía';


--
-- TOC entry 5502 (class 0 OID 0)
-- Dependencies: 220
-- Name: COLUMN companies.phone; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.companies.phone IS 'Número de teléfono';


--
-- TOC entry 5503 (class 0 OID 0)
-- Dependencies: 220
-- Name: COLUMN companies.email; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.companies.email IS 'Correo electrónico';


--
-- TOC entry 5504 (class 0 OID 0)
-- Dependencies: 220
-- Name: COLUMN companies.is_active; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.companies.is_active IS 'Indica si la compañía está activa';


--
-- TOC entry 5505 (class 0 OID 0)
-- Dependencies: 220
-- Name: COLUMN companies.updated_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.companies.updated_at IS 'Fecha de última actualización';


--
-- TOC entry 226 (class 1259 OID 175336)
-- Name: company_settings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.company_settings (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    company_id uuid NOT NULL,
    setting_key character varying(100) NOT NULL,
    setting_value text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "CompanyId1" uuid
);


ALTER TABLE public.company_settings OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 175355)
-- Name: company_subscriptions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.company_subscriptions (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    company_id uuid NOT NULL,
    plan_type character varying(50) NOT NULL,
    start_date timestamp with time zone NOT NULL,
    end_date timestamp with time zone NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "CompanyId1" uuid
);


ALTER TABLE public.company_subscriptions OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 175282)
-- Name: customers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.customers (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    full_name character varying(100),
    email character varying(100),
    phone character varying(20),
    loyalty_points integer DEFAULT 0,
    notes text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.customers OWNER TO postgres;

--
-- TOC entry 236 (class 1259 OID 175541)
-- Name: inventory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inventory (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    branch_id uuid,
    product_id uuid,
    quantity numeric(18,2) NOT NULL,
    unit character varying(20),
    min_threshold numeric(18,2) DEFAULT 0,
    last_updated timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    stock integer,
    min_stock integer,
    max_stock integer,
    company_id uuid,
    expiration_date timestamp with time zone,
    manufacturing_date timestamp with time zone,
    lot_number character varying(100),
    batch_number character varying(100),
    is_perishable boolean DEFAULT false,
    days_until_expiration integer,
    "Barcode" character varying(50),
    "ExpiryDate" timestamp with time zone,
    "IsActive" boolean DEFAULT false NOT NULL,
    "Location" character varying(100),
    "Notes" character varying(500),
    "ReorderPoint" numeric(18,2),
    "ReorderQuantity" numeric(18,2),
    "TotalValue" numeric(18,2),
    "UnitCost" numeric(18,2),
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.inventory OWNER TO postgres;

--
-- TOC entry 246 (class 1259 OID 175783)
-- Name: inventory_histories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inventory_histories (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    inventory_id uuid,
    product_id uuid,
    branch_id uuid,
    company_id uuid,
    quantity numeric(18,2) NOT NULL,
    quantity_before numeric(18,2),
    quantity_after numeric(18,2),
    unit character varying(20),
    expiration_date timestamp with time zone,
    manufacturing_date timestamp with time zone,
    lot_number character varying(100),
    batch_number character varying(100),
    type character varying(50) NOT NULL,
    reason character varying(500),
    notes text,
    created_by_user_id uuid,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.inventory_histories OWNER TO postgres;

--
-- TOC entry 252 (class 1259 OID 175997)
-- Name: inventory_movements; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inventory_movements (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    inventory_id uuid NOT NULL,
    product_id uuid NOT NULL,
    branch_id uuid NOT NULL,
    user_id uuid,
    type public.movement_type_enum NOT NULL,
    quantity numeric(18,2) NOT NULL,
    previous_stock numeric(18,2),
    new_stock numeric(18,2),
    reason character varying(500),
    reference character varying(100),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.inventory_movements OWNER TO postgres;

--
-- TOC entry 238 (class 1259 OID 175579)
-- Name: invoices; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.invoices (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    order_id uuid,
    customer_id uuid,
    total numeric(10,2),
    tax numeric(10,2),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.invoices OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 175680)
-- Name: journal_entries; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.journal_entries (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    entry_number character varying(20) NOT NULL,
    entry_date timestamp with time zone NOT NULL,
    type integer NOT NULL,
    description character varying(200) NOT NULL,
    reference character varying(500),
    status integer DEFAULT 1 NOT NULL,
    posted_at timestamp with time zone,
    posted_by character varying(100),
    total_debit numeric(18,2) NOT NULL,
    total_credit numeric(18,2) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone,
    created_by character varying(100),
    updated_by character varying(100),
    order_id uuid,
    payment_id uuid,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.journal_entries OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 175711)
-- Name: journal_entry_details; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.journal_entry_details (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    journal_entry_id uuid NOT NULL,
    account_id uuid NOT NULL,
    debit_amount numeric(18,2) NOT NULL,
    credit_amount numeric(18,2) NOT NULL,
    description character varying(500),
    reference character varying(100)
);


ALTER TABLE public.journal_entry_details OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 175292)
-- Name: modifiers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.modifiers (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(50),
    extra_cost numeric(10,2) DEFAULT 0
);


ALTER TABLE public.modifiers OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 175596)
-- Name: notifications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.notifications (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    order_id uuid,
    message text,
    is_read boolean DEFAULT false,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.notifications OWNER TO postgres;

--
-- TOC entry 240 (class 1259 OID 175611)
-- Name: order_cancellation_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.order_cancellation_logs (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    order_id uuid NOT NULL,
    user_id uuid,
    supervisor_id uuid,
    reason text NOT NULL,
    date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    products text NOT NULL
);


ALTER TABLE public.order_cancellation_logs OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 175635)
-- Name: order_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.order_items (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    order_id uuid,
    product_id uuid,
    quantity numeric(10,2) NOT NULL,
    unit_price numeric(10,2) NOT NULL,
    discount numeric(10,2) DEFAULT 0,
    notes text,
    status text NOT NULL,
    prepared_by_station_id uuid,
    prepared_at timestamp with time zone,
    "KitchenStatus" integer NOT NULL,
    "SentAt" timestamp with time zone,
    "StationId" uuid,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.order_items OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 175512)
-- Name: orders; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orders (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    "OrderNumber" character varying(50) NOT NULL,
    table_id uuid,
    customer_id uuid,
    user_id uuid,
    order_type character varying(20) NOT NULL,
    status character varying(20) NOT NULL,
    total_amount numeric(10,2),
    opened_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    closed_at timestamp with time zone,
    notes character varying(500),
    company_id uuid,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.orders OWNER TO postgres;

--
-- TOC entry 242 (class 1259 OID 175664)
-- Name: payments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.payments (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    order_id uuid,
    method character varying(30),
    amount numeric(10,2) NOT NULL,
    paid_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    is_voided boolean DEFAULT false,
    is_shared boolean DEFAULT false NOT NULL,
    payer_name character varying(100),
    status character varying(20) DEFAULT 'COMPLETED'::character varying NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.payments OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 175299)
-- Name: product_categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.product_categories (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.product_categories OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 175564)
-- Name: product_modifiers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.product_modifiers (
    product_id uuid NOT NULL,
    modifier_id uuid NOT NULL
);


ALTER TABLE public.product_modifiers OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 175455)
-- Name: products; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.products (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(500),
    price numeric(18,2) NOT NULL,
    cost numeric(18,2),
    tax_rate numeric(5,2) DEFAULT 0.07,
    unit character varying(20) DEFAULT 'unit'::character varying,
    image_url character varying(500),
    is_active boolean DEFAULT true,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    category_id uuid,
    station_id uuid,
    "Stock" numeric,
    company_id uuid,
    "ProductCategoryId" uuid,
    is_perishable boolean DEFAULT false,
    shelf_life_days integer,
    alert_days_before_expiration integer DEFAULT 30,
    storage_conditions character varying(500),
    requires_lot_tracking boolean DEFAULT false,
    requires_expiration_date boolean DEFAULT false,
    "SupplierId" uuid,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.products OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 175873)
-- Name: purchase_order_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_order_items (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    purchase_order_id uuid,
    product_id uuid,
    product_name character varying(200),
    product_description text,
    quantity numeric(18,2) NOT NULL,
    unit_price numeric(18,2) NOT NULL,
    total_price numeric(18,2) NOT NULL,
    unit character varying(20),
    tax_rate numeric(5,2),
    tax_amount numeric(18,2),
    discount_rate numeric(5,2),
    discount_amount numeric(18,2),
    received_quantity numeric(18,2) DEFAULT 0,
    pending_quantity numeric(18,2) DEFAULT 0,
    expected_expiration_date timestamp with time zone,
    lot_number character varying(100),
    notes text,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.purchase_order_items OWNER TO postgres;

--
-- TOC entry 251 (class 1259 OID 175925)
-- Name: purchase_order_receipt_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_order_receipt_items (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    purchase_order_receipt_id uuid,
    purchase_order_item_id uuid,
    product_id uuid,
    product_name character varying(200),
    ordered_quantity numeric(18,2) NOT NULL,
    received_quantity numeric(18,2) NOT NULL,
    accepted_quantity numeric(18,2),
    rejected_quantity numeric(18,2),
    unit_price numeric(18,2) NOT NULL,
    total_price numeric(18,2) NOT NULL,
    unit character varying(20),
    expiration_date timestamp with time zone,
    manufacturing_date timestamp with time zone,
    lot_number character varying(100),
    batch_number character varying(100),
    quality_notes text,
    rejection_reason text,
    status character varying(50) DEFAULT 'Pending'::character varying NOT NULL
);


ALTER TABLE public.purchase_order_receipt_items OWNER TO postgres;

--
-- TOC entry 250 (class 1259 OID 175893)
-- Name: purchase_order_receipts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_order_receipts (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    purchase_order_id uuid,
    receipt_number character varying(50) NOT NULL,
    receipt_date timestamp with time zone NOT NULL,
    received_by_user_id uuid,
    branch_id uuid,
    company_id uuid,
    status character varying(50) DEFAULT 'Draft'::character varying NOT NULL,
    notes text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone
);


ALTER TABLE public.purchase_order_receipts OWNER TO postgres;

--
-- TOC entry 248 (class 1259 OID 175838)
-- Name: purchase_orders; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_orders (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    order_number character varying(50) NOT NULL,
    branch_id uuid,
    company_id uuid,
    supplier_id uuid,
    created_by_user_id uuid,
    order_date timestamp with time zone NOT NULL,
    expected_delivery_date timestamp with time zone,
    actual_delivery_date timestamp with time zone,
    status character varying(50) DEFAULT 'Draft'::character varying NOT NULL,
    total_amount numeric(18,2) DEFAULT 0,
    tax_amount numeric(18,2) DEFAULT 0,
    discount_amount numeric(18,2) DEFAULT 0,
    notes text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.purchase_orders OWNER TO postgres;

--
-- TOC entry 244 (class 1259 OID 175700)
-- Name: split_payments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.split_payments (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    payment_id uuid,
    person_name character varying(100),
    amount numeric(10,2),
    method character varying(30)
);


ALTER TABLE public.split_payments OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 175406)
-- Name: stations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.stations (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(100) NOT NULL,
    type character varying(50) NOT NULL,
    icon character varying(50),
    area_id uuid,
    is_active boolean DEFAULT true NOT NULL,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.stations OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 175823)
-- Name: suppliers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.suppliers (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(200) NOT NULL,
    contact_person character varying(200),
    email character varying(200),
    phone character varying(50),
    address text,
    city character varying(100),
    state character varying(100),
    postal_code character varying(20),
    country character varying(100),
    tax_id character varying(50),
    payment_terms character varying(200),
    lead_time_days integer,
    is_active boolean DEFAULT true,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone,
    company_id uuid,
    "Description" character varying(500),
    "ContactPerson" character varying(100),
    "Email" character varying(100),
    "Phone" character varying(20),
    "Fax" character varying(20),
    "Address" character varying(200),
    "City" character varying(100),
    "State" character varying(50),
    "PostalCode" character varying(20),
    "Country" character varying(100),
    "TaxId" character varying(50),
    "AccountNumber" character varying(50),
    "Website" character varying(100),
    "Notes" character varying(500),
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.suppliers OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 175418)
-- Name: tables; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tables (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    area_id uuid,
    table_number character varying(10),
    capacity integer,
    status character varying(20) DEFAULT 'AVAILABLE'::character varying,
    is_active boolean DEFAULT true,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.tables OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 175487)
-- Name: user_assignments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_assignments (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    station_id uuid,
    area_id uuid,
    assigned_table_ids jsonb,
    assigned_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    unassigned_at timestamp with time zone,
    is_active boolean DEFAULT true NOT NULL,
    notes character varying(500),
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.user_assignments OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 175386)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    branch_id uuid,
    "Username" text,
    "FirstName" text,
    "LastName" text,
    full_name character varying(100),
    email character varying(100) NOT NULL,
    password_hash text NOT NULL,
    role public.user_role_enum NOT NULL,
    is_active boolean DEFAULT true,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    company_id uuid,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255)
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 5477 (class 0 OID 176045)
-- Dependencies: 254
-- Data for Name: PurchaseOrderItems; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."PurchaseOrderItems" ("Id", "PurchaseOrderId", "ProductId", "UnitPrice", "Quantity", "Subtotal", "TaxRate", "TaxAmount", "TotalAmount", "ReceivedQuantity", "Notes", "IsActive", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- TOC entry 5476 (class 0 OID 176038)
-- Dependencies: 253
-- Data for Name: PurchaseOrders; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."PurchaseOrders" ("Id", "OrderNumber", "SupplierId", "CompanyId", "BranchId", "CreatedById", "OrderDate", "ExpectedDeliveryDate", "ActualDeliveryDate", "Subtotal", "TaxAmount", "TotalAmount", "Notes", "Status", "IsActive", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- TOC entry 5479 (class 0 OID 176088)
-- Dependencies: 256
-- Data for Name: TransferItems; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TransferItems" ("Id", "TransferId", "ProductId", "UnitPrice", "Quantity", "Subtotal", "TaxRate", "TaxAmount", "TotalAmount", "ReceivedQuantity", "Notes", "IsActive", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- TOC entry 5478 (class 0 OID 176081)
-- Dependencies: 255
-- Data for Name: Transfers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Transfers" ("Id", "TransferNumber", "SourceBranchId", "DestinationBranchId", "CompanyId", "CreatedById", "ApprovedById", "ReceivedById", "TransferDate", "ExpectedDeliveryDate", "ActualDeliveryDate", "Subtotal", "TaxAmount", "TotalAmount", "Notes", "Status", "IsActive", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- TOC entry 5441 (class 0 OID 175209)
-- Dependencies: 218
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20250720180020_InitialDatabaseClean	9.0.5
20250720181428_UpdateUserData	9.0.5
20250804162434_AddInventoryMovements	9.0.5
AddSuppliers	9.0.5
20250804194535_AddPurchaseOrders	9.0.5
\.


--
-- TOC entry 5442 (class 0 OID 175257)
-- Dependencies: 219
-- Data for Name: accounts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.accounts (id, code, name, description, type, category, nature, parent_account_id, is_active, is_system, created_at, updated_at, created_by, updated_by, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5451 (class 0 OID 175373)
-- Dependencies: 228
-- Data for Name: areas; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.areas (id, branch_id, name, "Description", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5455 (class 0 OID 175431)
-- Dependencies: 232
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.audit_logs (id, user_id, action, table_name, record_id, "timestamp", company_id, branch_id, old_values, new_values, ip_address, user_agent, "CompanyId", "BranchId", "LogLevel", "Module", "Description", "OldValues", "NewValues", "ErrorDetails", "IpAddress", "UserAgent", "SessionId", "IsError", "ErrorCode", "ExceptionType", "StackTrace", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
16cebd13-43b0-46a5-a903-28d4f76c9621	\N	REQUEST_START	\N	\N	2025-08-05 18:20:15.476636-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
01996dce-1f90-4545-a711-3c03235bafc1	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:20:17.488622-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2355ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1a189271-580e-4217-b1c3-2ed8c3a208a8	\N	REQUEST_START	\N	\N	2025-08-05 18:20:26.806666-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
37adaa29-a33b-48c9-81f9-b2010a39ee96	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:20:27.571062-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 678ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d58ce6f7-fd8e-4d32-b610-39a3974c8cc2	\N	REQUEST_START	\N	\N	2025-08-05 18:20:27.627747-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1d2a9ae3-790a-45b6-8864-fd99d8f1db42	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:20:27.8328-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 183ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1320413e-a1c9-4b77-bf5a-e46f83c6e3ad	\N	REQUEST_START	\N	\N	2025-08-05 18:20:40.014481-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
05361609-0f55-443e-b0f1-2a19fbc85516	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:20:40.133233-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 108ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
aa4d1e41-69d3-48a5-920d-334b055618ba	\N	REQUEST_START	\N	\N	2025-08-05 18:20:40.211392-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7760b949-515d-4f11-99b9-b364b74fe993	\N	REQUEST_START	\N	\N	2025-08-05 18:20:40.211434-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f2bfac6f-eb85-4961-9d64-12acad84c5f7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:20:40.429746-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 228ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
bf5c3e5a-7759-4978-b6b7-e26b58fd9b84	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:20:40.470372-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 269ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2df6d5b8-2ef2-4a56-8c3e-071865c46925	\N	REQUEST_START	\N	\N	2025-08-05 18:20:43.806884-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fe8ee44d-36b7-4bf7-bb86-bd5040b9d506	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:20:43.863413-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 46ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e6a7f3a8-ed87-4478-8270-88fd8a2add4c	\N	REQUEST_START	\N	\N	2025-08-05 18:20:53.88198-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6f8b4f12-8b05-4ad1-8d75-79b4fb57613c	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:20:54.00223-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 101ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
87b20c62-7484-4eea-988c-e3080c3b06bb	\N	REQUEST_START	\N	\N	2025-08-05 18:21:00.085218-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: DELETE /Company/Delete	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d74b267c-9cc7-4d8a-8f33-c679306cd867	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:21:00.29775-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: DELETE /Company/Delete - Status: 200 - Tiempo: 196ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ac316933-c19a-422c-ade9-12facc341af8	\N	REQUEST_START	\N	\N	2025-08-05 18:40:50.389064-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
270c4ed3-a563-40b3-b867-f74a4770099f	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:40:52.648379-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2487ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
33fcb678-3ac7-452c-9a03-3df240a2fcfc	\N	REQUEST_START	\N	\N	2025-08-05 18:40:58.788617-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d9c45d99-62dd-456e-a748-3a6ea47eabff	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:40:59.451646-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 620ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
14247b82-4d85-4121-876c-a330e20e26dc	\N	REQUEST_START	\N	\N	2025-08-05 18:40:59.496877-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d56ba782-86fa-4eac-acab-d0591e07b252	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:40:59.685635-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 161ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
39fc5430-f44c-4b87-92dc-68642bb38021	\N	REQUEST_START	\N	\N	2025-08-05 18:41:07.074338-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f8b9fe6e-f94e-47aa-91e4-e3888d7c2306	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:41:07.1951-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 110ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
910246bf-7362-41d5-be4b-83ad4437d7e3	\N	REQUEST_START	\N	\N	2025-08-05 18:41:07.306591-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
132659ba-d26c-4bc2-b7b3-91d39d9da2a9	\N	REQUEST_START	\N	\N	2025-08-05 18:41:07.306591-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d28c5b40-9b61-4f99-aaf6-58d8f959c0bb	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:41:07.508546-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 231ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b3748e4e-14ef-40b0-a5fa-e3a6db320ae5	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:41:07.548605-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 271ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
728f3d32-265f-4a67-9dfa-639c6ae39c28	\N	REQUEST_START	\N	\N	2025-08-05 18:43:58.695818-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /.well-known/appspecific/com.chrome.devtools.json	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
bb254652-eece-4aab-8a9b-5ddc21b88063	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:43:58.772702-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /.well-known/appspecific/com.chrome.devtools.json - Status: 404 - Tiempo: 51ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
92453225-27af-46c5-9b80-49d837bab36c	\N	REQUEST_START	\N	\N	2025-08-05 18:47:47.928863-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3a789552-3dc5-489a-ba41-a6c518c7f57d	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:47:49.936959-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2234ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c920b4d0-cb2c-4002-9ae4-4601e969b27d	\N	REQUEST_START	\N	\N	2025-08-05 18:47:55.93831-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
84ed4bce-7ca7-4f14-8a9f-72433ec24f0e	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:47:56.692988-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 699ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a9d8f183-8720-4edc-94d2-0180142cdc40	\N	REQUEST_START	\N	\N	2025-08-05 18:47:56.732476-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
51702fea-5179-4f54-a065-5872db8bec42	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:47:56.958365-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 190ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
31482857-b9e7-4ec0-83b4-edd34237e5dd	\N	REQUEST_START	\N	\N	2025-08-05 18:48:02.314599-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
986fe34b-042d-47c3-bde0-be9cda6dea60	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:48:02.438732-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 112ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5f6a1bbc-b2e4-4f35-99d1-cfc19c8b75a4	\N	REQUEST_START	\N	\N	2025-08-05 18:48:02.540514-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a542e25d-0aff-41e3-83ef-5b1ac245e921	\N	REQUEST_START	\N	\N	2025-08-05 18:48:02.540509-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9a19fb44-c1a6-4c36-a6ef-158cd41ce2d8	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:48:02.732678-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 212ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5e3705ef-ec88-4af0-8c96-64109ef15730	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:48:02.777339-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 255ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d01cdd60-7c90-4d39-8a64-efd7ed34b8ec	\N	REQUEST_START	\N	\N	2025-08-05 18:50:50.702685-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
89488dc1-2bea-4ea2-ad29-48b627d20b0b	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:50:52.586663-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2103ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
068a31d4-710a-41c4-bd91-febce3a68bf4	\N	REQUEST_START	\N	\N	2025-08-05 18:50:58.416378-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ea043382-85f1-4059-acae-4679fe1edc92	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:50:59.040458-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 584ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
89f47c9b-e998-4b07-a7b7-0383fee419ad	\N	REQUEST_START	\N	\N	2025-08-05 18:50:59.091938-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a934ef96-779f-451d-8130-0ce0332d580e	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:50:59.280892-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 158ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
bdb0e050-dcbc-4612-bb8f-047560c0a380	\N	REQUEST_START	\N	\N	2025-08-05 18:51:06.194071-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b2db01bc-66bf-4039-ada4-22320de9bdc4	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:51:06.305002-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 118ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
dbd0684f-99a0-4c56-bbb0-364f225b46c2	\N	REQUEST_START	\N	\N	2025-08-05 18:51:06.400277-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c42eda22-4cf9-42fe-9420-5d7f3f057e7f	\N	REQUEST_START	\N	\N	2025-08-05 18:51:06.400294-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f69062a1-131a-4d7a-b70f-7a72e26e3868	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:51:06.588913-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 208ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
87ac3489-6dac-4c35-b0de-7fa897d299a5	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:51:06.628374-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 248ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6d34c85a-34b7-43ca-94ea-7fb9206f5f8a	\N	REQUEST_START	\N	\N	2025-08-05 18:51:14.004423-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /.well-known/appspecific/com.chrome.devtools.json	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
86557a61-9d7b-4a28-a2ab-97cec6de3709	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:51:14.055668-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /.well-known/appspecific/com.chrome.devtools.json - Status: 404 - Tiempo: 30ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
90bc4864-5413-4f41-aeb8-ef6dd6da1b32	\N	REQUEST_START	\N	\N	2025-08-05 18:54:28.61174-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fb8efc3d-d14e-4308-b2ff-c5e21c35bd84	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:54:30.489277-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2021ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d311e374-8592-48fc-9425-3efc141c4e04	\N	REQUEST_START	\N	\N	2025-08-05 18:54:36.44969-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5930882e-1167-4cf9-8bd3-1e996188ceaf	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:54:37.11967-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 626ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c7ea3800-6e32-4be2-bdf9-975314e7e45a	\N	REQUEST_START	\N	\N	2025-08-05 18:54:37.161125-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0b10a3ff-5a45-412b-a6c8-67d9ff88555a	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:54:37.353898-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 164ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
577e4d58-3251-4a79-a2e3-5640788eeec7	\N	REQUEST_START	\N	\N	2025-08-05 18:54:45.180872-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f1bb2c9f-9a49-4daf-8803-38db06d4082d	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:54:45.30856-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 116ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b76c0425-c0d6-4d97-85a8-1020d25bae71	\N	REQUEST_START	\N	\N	2025-08-05 18:54:45.407429-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
814b0755-ac09-4ce2-8d89-8df6d634e94b	\N	REQUEST_START	\N	\N	2025-08-05 18:54:45.407429-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
37d02a17-344d-49c1-81c7-f6faf1c8b823	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:54:45.613499-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 236ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0f13d279-a904-4f5d-84a6-ad35bc5cd7f9	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:54:45.64978-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 270ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a31cc8ba-a329-49e3-a39e-788195d43adb	\N	REQUEST_START	\N	\N	2025-08-05 18:59:08.836933-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7f6ed781-f787-4bb9-a8d7-559b7a088441	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:59:10.685841-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2006ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8e0a62e0-b6ec-4aa3-8513-7ed9aabe5420	\N	REQUEST_START	\N	\N	2025-08-05 18:59:16.211079-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7ce3a75f-294e-4098-9cde-ee567ee29217	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 18:59:16.872171-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 613ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6ce28ac0-0b68-4282-b881-4c89b1795dd7	\N	REQUEST_START	\N	\N	2025-08-05 18:59:16.93026-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3bfb5708-983c-46cc-977c-f081e49c65d8	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:59:17.131041-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 174ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6f2d2087-a263-4e46-bc58-eb8f346ed659	\N	REQUEST_START	\N	\N	2025-08-05 18:59:21.150152-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /StationOrders/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
72de13f8-3807-4185-93f2-1bcaf6c15727	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:59:21.258595-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /StationOrders/Index - Status: 400 - Tiempo: 102ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6c81510b-3ed7-4c34-810f-4869be528b52	\N	REQUEST_START	\N	\N	2025-08-05 18:59:22.844648-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6445c63f-4991-413c-a375-768151605bad	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 18:59:22.903065-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 44ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
443bf3c4-7b28-4973-b266-c87eb04f02a4	\N	REQUEST_START	\N	\N	2025-08-05 19:00:05.589531-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Station/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
98f2280d-c68b-4033-9290-b169bc9aedd7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:00:05.715277-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Station/Index - Status: 200 - Tiempo: 109ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d54e7329-4c75-4f30-9381-041edd7798b6	\N	REQUEST_START	\N	\N	2025-08-05 19:00:05.806118-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Station/GetAreas	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e787410f-1e84-416c-a95d-ca9acdf382ce	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:00:05.924122-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Station/GetAreas - Status: 200 - Tiempo: 118ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3ce43ed9-43d1-4220-81c8-970b269c3a96	\N	REQUEST_START	\N	\N	2025-08-05 19:00:10.338046-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Station/GetAreas	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
80349363-69d8-4505-be7b-88de8b730ca1	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:00:10.389586-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Station/GetAreas - Status: 200 - Tiempo: 35ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d0f84ee6-10bb-4687-9b7f-0bb99aee3321	\N	REQUEST_START	\N	\N	2025-08-05 19:00:13.246678-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0874827c-9e0a-43c2-9df5-8729fe4e4029	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:00:13.299931-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 39ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
dc5fb779-cdf0-4ac1-a1b1-a24a04c0997b	\N	REQUEST_START	\N	\N	2025-08-05 19:00:16.490489-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d3a0243e-6456-45e1-8148-21d6d9478988	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:00:16.574941-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 73ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b5bdedae-6b3d-4d0e-a06e-91ab31b015fc	\N	REQUEST_START	\N	\N	2025-08-05 19:00:16.664833-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
76901d77-589a-4456-9d1c-6253f3f14753	\N	REQUEST_START	\N	\N	2025-08-05 19:00:16.664756-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
588969ce-a40c-42c2-b063-228567d3635e	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:00:16.854452-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 214ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f48a515a-3a2f-4b51-a348-30b7c6f4b42f	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:00:16.89385-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 254ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fd15e511-724c-4149-bc80-0fb35f01c2e8	\N	REQUEST_START	\N	\N	2025-08-05 19:02:13.76141-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1cb7d44f-efa4-428d-9885-0c3e77333a38	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:02:13.817062-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 44ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9b5d1876-7fad-4884-b322-fb1756d3e917	\N	REQUEST_START	\N	\N	2025-08-05 19:02:13.938852-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d42c132f-62c0-45e2-aa9c-dc5400ab3d22	\N	REQUEST_START	\N	\N	2025-08-05 19:02:13.928239-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8f0ae348-d8e6-4413-b3f5-0c87722432e7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:02:30.734445-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 12945ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
54c1dc6f-ed9e-4a68-92ca-1860bae890cf	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:03:57.694248-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 103771ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
924b48a5-57f8-4a6b-b8f5-3ae757f3d25e	\N	REQUEST_START	\N	\N	2025-08-05 19:06:15.708537-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
51f8af12-8f7a-4779-913f-589c0a58a1bb	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:06:17.477653-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2155ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
cd16e2ae-1a6c-4c9e-827c-532d2674fe71	\N	REQUEST_START	\N	\N	2025-08-05 19:06:26.116734-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
428241a3-42b0-4ffa-a396-2128ade38b3b	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:06:26.775767-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 590ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8b704557-4e10-40d8-beab-ec76e87b6177	\N	REQUEST_START	\N	\N	2025-08-05 19:06:26.821733-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1edeac55-ff02-429e-bcce-a7b2b0b7247c	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:06:27.017327-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 168ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9bdc56c8-58ee-47fc-8eff-a4812dc68430	\N	REQUEST_START	\N	\N	2025-08-05 19:06:33.151978-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
11be3624-b44c-42ab-9ad6-24f323542dd0	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:06:33.259066-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 112ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2736d733-1b94-4e05-b982-350ca055bb70	\N	REQUEST_START	\N	\N	2025-08-05 19:06:33.3368-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
72004ee3-106a-4f3b-ad9d-aae62fb6a859	\N	REQUEST_START	\N	\N	2025-08-05 19:06:33.357581-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6be49219-c49d-47ae-90da-5b3735dbe5a4	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:06:42.577407-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 9223ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4d4a2c97-6243-48b3-9ba1-c746776b71bf	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:06:42.607223-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 9280ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7f9bcca6-f9d6-459a-8d70-53b663348649	\N	REQUEST_START	\N	\N	2025-08-05 19:06:46.810588-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /.well-known/appspecific/com.chrome.devtools.json	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4396eef3-ad17-4834-b681-9373fd160446	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:06:46.86972-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /.well-known/appspecific/com.chrome.devtools.json - Status: 404 - Tiempo: 30ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ed89b199-a25d-4eab-b225-7cd8da53052b	\N	REQUEST_START	\N	\N	2025-08-05 19:07:10.960137-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
40981a92-986e-42dd-8984-ee4c353e07c4	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:07:11.030708-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 57ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8db090a7-4e06-4a64-81dd-d7fcafc1c5bf	\N	REQUEST_START	\N	\N	2025-08-05 19:07:11.059016-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /.well-known/appspecific/com.chrome.devtools.json	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ee681d6c-293e-4434-9681-8bfe2ed20943	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:07:11.117236-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /.well-known/appspecific/com.chrome.devtools.json - Status: 404 - Tiempo: 48ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
99631821-306d-4632-8dd8-b2a25c8ba0f6	\N	REQUEST_START	\N	\N	2025-08-05 19:07:11.202833-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d091be30-5f1b-4b8c-8484-9983a07cfba8	\N	REQUEST_START	\N	\N	2025-08-05 19:07:11.202833-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5f8ae621-38a4-4ab6-a02d-a6a9a260a700	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:07:12.888832-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 1711ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
35c39ed4-c0a6-4ee2-8a5f-20352b8d49f1	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:07:12.922743-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 1735ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5a6a6bbf-fc0a-4178-b267-b4bd71df14cf	\N	REQUEST_START	\N	\N	2025-08-05 19:11:01.544559-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
dfa6c1cf-800b-4bdb-b016-67e39f15358c	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:03.511635-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2160ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
775b8dec-7654-4eee-9678-2699692fee08	\N	REQUEST_START	\N	\N	2025-08-05 19:11:10.70963-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2c98da1d-0748-4767-82b1-6b9b6396e037	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:11.373465-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 612ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4a83170e-7923-4b6c-8628-9988a2e77e20	\N	REQUEST_START	\N	\N	2025-08-05 19:11:11.425569-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ad3c4a5e-bf3a-422d-9b0a-d51310633aa7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:11.623984-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 161ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
099b54c8-fa0f-44e6-8bfd-dd3fe699ba64	\N	REQUEST_START	\N	\N	2025-08-05 19:11:18.65064-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e918d778-5916-4f37-baef-a2edb23e9b87	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:18.764464-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 101ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a5b004d9-ddf5-4562-9f52-f8a906ddc7d8	\N	REQUEST_START	\N	\N	2025-08-05 19:11:18.843497-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
467149aa-01a1-4a68-b422-12697faafbce	\N	REQUEST_START	\N	\N	2025-08-05 19:11:18.864048-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
24a69789-be64-4d94-9c4a-ed5a9a7aa716	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:20.931411-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 2061ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2c2da382-a8bd-418a-9ae8-5dd744bf8a56	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:20.97237-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 2124ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c21df968-2856-4ee0-9226-915a3d7ad902	\N	REQUEST_START	\N	\N	2025-08-05 19:11:33.581233-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c48b9cc0-b046-4158-91ab-cd0f5ec35174	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:33.73156-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 123ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
784dfeb7-ffb7-4b2f-8d54-f124b2c94452	\N	REQUEST_START	\N	\N	2025-08-05 19:11:39.193536-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /User/Update	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4906fc9b-f1d5-4cf1-af26-1cc7b1e5b998	770e8400-e29b-41d4-a716-446655440001	UPDATE	users	770e8400-e29b-41d4-a716-446655440002	2025-08-05 19:11:39.367318-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	USER	Usuario actualizado: María González (gerente@restbar.com)	{\r\n  "FullName": "Mar\\u00EDa Gonz\\u00E1lez",\r\n  "Email": "gerente@restbar.com",\r\n  "Role": 5,\r\n  "BranchId": "660e8400-e29b-41d4-a716-446655440001",\r\n  "IsActive": true\r\n}	{\r\n  "FullName": "Mar\\u00EDa Gonz\\u00E1lez",\r\n  "Email": "gerente@restbar.com",\r\n  "Role": 6,\r\n  "BranchId": "660e8400-e29b-41d4-a716-446655440001",\r\n  "IsActive": true\r\n}	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	ef4888b8-131a-f5d6-0d46-49b3f81f3f3f	f	\N	\N	\N	\N	\N	\N	\N
ca126303-dada-46fa-9678-37eb23b49890	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:39.403186-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: POST /User/Update - Status: 200 - Tiempo: 202ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5c980b87-d0c5-416e-a466-f4672e188d79	\N	REQUEST_START	\N	\N	2025-08-05 19:11:39.440146-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5173b356-b4e2-4681-bc66-4e4b6975c96a	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:41.087893-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 1638ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f88ac877-9c86-430e-9336-e4b8bbceba2e	\N	REQUEST_START	\N	\N	2025-08-05 19:11:49.670575-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: DELETE /User/Delete	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
efa78e6b-e890-4b26-9629-4268a1189789	770e8400-e29b-41d4-a716-446655440001	DELETE	users	770e8400-e29b-41d4-a716-446655440002	2025-08-05 19:11:49.720296-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	USER	Usuario eliminado: María González (gerente@restbar.com)	{\r\n  "FullName": "Mar\\u00EDa Gonz\\u00E1lez",\r\n  "Email": "gerente@restbar.com",\r\n  "Role": 6,\r\n  "BranchId": "660e8400-e29b-41d4-a716-446655440001",\r\n  "IsActive": true\r\n}	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	11a7f617-3dcb-8f5e-e85a-934012a310b8	f	\N	\N	\N	\N	\N	\N	\N
e5e9105c-60de-4251-ba3c-424e4a2df48c	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:49.845782-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: DELETE /User/Delete - Status: 200 - Tiempo: 154ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8cce9f79-b51c-4166-807c-ae57510f9de3	\N	REQUEST_START	\N	\N	2025-08-05 19:11:49.880714-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e5a87a4e-b112-4161-80b2-c15094fcdbee	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:11:51.489207-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 1604ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
48b4d853-59e2-4d54-bb63-14e50cb01e66	\N	REQUEST_START	\N	\N	2025-08-05 19:24:08.418978-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
015ed6bf-db6a-4285-ba03-31112166cd47	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:10.219727-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2068ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4837087c-a7cd-45b0-a939-22eb7d4eddee	\N	REQUEST_START	\N	\N	2025-08-05 19:24:15.705243-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6b59ae61-b9d5-4647-aafe-4913f7bb7264	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:16.337565-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 594ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e6868767-b2d4-431b-89c9-3e888a8c8164	\N	REQUEST_START	\N	\N	2025-08-05 19:24:16.400128-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
abc119ba-004a-454e-a26e-806a2262ab80	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:16.601743-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 196ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4cede94a-933c-4bbe-b5c9-14cca5fc87ca	\N	REQUEST_START	\N	\N	2025-08-05 19:24:21.723013-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b13025c1-49fc-41d0-9ed9-fd7b69a880d6	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:21.836179-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 104ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
87b80b04-ed93-49b6-9711-e347a69082a8	\N	REQUEST_START	\N	\N	2025-08-05 19:24:21.920396-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f0b886db-ea8a-4d24-a7a5-7282261df3f6	\N	REQUEST_START	\N	\N	2025-08-05 19:24:21.939329-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
40a498f0-26f6-48e4-95f0-2c1b6f00a323	\N	REQUEST_START	\N	\N	2025-08-05 19:24:21.92038-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetCompanies	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c6e1657a-1570-4c9c-813c-bb86b33ef8e1	\N	REQUEST_START	\N	\N	2025-08-05 19:24:42.448269-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
675e04d7-766e-4e9e-86f9-96434fd00ac7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:42.525094-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 65ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2ddad5d9-927b-451d-9a38-beca1cf65257	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:54.234099-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /.well-known/appspecific/com.chrome.devtools.json - Status: 404 - Tiempo: 27ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9f4c75dd-bf5f-46f5-8c48-f1f4a8f5d42c	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:23.725834-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetCompanies - Status: 200 - Tiempo: 1809ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
89e0beb5-fa8e-4d19-bf07-df0680da6ba6	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:23.767673-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 1871ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a366ff95-6860-4ba2-872e-f242bf06e7fd	\N	REQUEST_START	\N	\N	2025-08-05 19:24:54.186736-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /.well-known/appspecific/com.chrome.devtools.json	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c19241bf-ebbb-4eab-8835-1b21aaa35071	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:23.725834-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 1810ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7612ef26-7d39-43cb-a075-333dc6939e1a	\N	REQUEST_START	\N	\N	2025-08-05 19:24:27.214281-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
23b25851-81eb-4024-a602-5d4044e5025b	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:24:27.365715-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 126ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8c4988c6-d977-4de8-a7be-b2dfef0855f3	\N	REQUEST_START	\N	\N	2025-08-05 19:36:29.601099-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
62bfe815-341e-42a4-96af-560986ac9fd0	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:31.342842-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 1887ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f9ea981f-e15e-4d22-911a-112a674895a0	\N	REQUEST_START	\N	\N	2025-08-05 19:36:36.899418-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a1cdb1fc-4dd4-4694-90f2-2a8d6da88e32	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:37.546363-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 606ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4be56269-6c96-4d98-ba85-26aab05266b1	\N	REQUEST_START	\N	\N	2025-08-05 19:36:37.598132-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8697d178-d02c-42a7-b75e-491b8cc97068	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:37.788296-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 167ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
81ee0e65-8e8d-4d34-a8f5-0a3b393203f4	\N	REQUEST_START	\N	\N	2025-08-05 19:36:46.439716-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
27d74d09-2f0a-47ae-9736-3c78afa84208	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:46.502053-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 65ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
cfb7e501-845d-40b9-81a7-77d2d40a15b0	\N	REQUEST_START	\N	\N	2025-08-05 19:36:53.222005-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
eb413f3e-cd44-4942-b149-9f81c88fe535	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:53.32442-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 88ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e79fbe9f-de5c-488b-bbdb-9485cc8944ea	\N	REQUEST_START	\N	\N	2025-08-05 19:36:53.409707-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetCompanies	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
85dce8a8-0d68-4f75-9036-f7fb0a72e52c	\N	REQUEST_START	\N	\N	2025-08-05 19:36:53.409703-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
35f990fb-8740-4b7c-9d5b-d19a5e50630e	\N	REQUEST_START	\N	\N	2025-08-05 19:36:53.431991-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
eabd5b02-9723-414d-a64b-f3afb63a06f1	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:55.252873-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetCompanies - Status: 200 - Tiempo: 1857ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
81546e49-d2c1-448c-baeb-1203a2ef76bd	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:55.321807-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 1884ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
14dcccf6-a92f-459c-bc94-65653b88b18f	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:55.352447-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 1964ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a688d836-8109-430c-8685-b9d074ff6395	\N	REQUEST_START	\N	\N	2025-08-05 19:36:58.927964-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a1981491-7859-43cb-8119-68838b018d34	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:59.086792-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 135ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5cd56bde-0438-4259-b3f2-7a4260944749	\N	REQUEST_START	\N	\N	2025-08-05 19:36:59.11334-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
98d1d503-1a23-490a-9d21-2dc0ffb25683	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:36:59.168293-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 42ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f26ebaeb-212b-4ec3-978a-50ece067baf8	\N	REQUEST_START	\N	\N	2025-08-05 19:37:03.331385-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
993e003b-3e27-4561-9dd5-6a07fdbd9955	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:37:03.378603-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 40ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2328eb86-1032-4746-a2da-df457d129a02	\N	REQUEST_START	\N	\N	2025-08-05 19:37:08.993078-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
28235b1c-fcdd-498c-b485-b9b772678578	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:37:09.059279-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 50ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
dffbc805-2e14-49d7-85fa-5580fe6838ff	\N	REQUEST_START	\N	\N	2025-08-05 19:37:22.542494-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
56c0f67c-5860-4dac-aa99-1c14ee480254	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:37:22.587118-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 40ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
bcc93acc-fe41-4341-8359-072838a0d199	\N	REQUEST_START	\N	\N	2025-08-05 19:37:30.008251-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ef4a3bb2-00cb-4559-b7d7-5c98220b76de	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:37:30.055328-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 36ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b7d8e78a-8eeb-4775-8829-7ef3f700ecca	\N	REQUEST_START	\N	\N	2025-08-05 19:37:34.079259-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
722d6b3c-3447-4c70-8683-273d40c70a73	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:37:37.910516-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 41ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
20b0261e-14eb-4223-aad7-9ccbdc2599be	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:37:34.135708-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 49ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d9071d1b-2623-428d-b929-8f62af3570ee	\N	REQUEST_START	\N	\N	2025-08-05 19:37:42.253389-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2a7c2f44-58e7-4a77-860b-b0ff1a0c44fd	\N	REQUEST_START	\N	\N	2025-08-05 19:37:37.857622-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c7c4ca0e-e15c-48b7-b509-6ef1f60f542d	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:37:42.299891-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 37ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a8173845-8a12-49e7-8ae5-4fdaf00900b7	\N	REQUEST_START	\N	\N	2025-08-05 19:47:22.996001-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
04ed1e3e-c706-423e-8385-b533ffec7041	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:47:25.006107-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2206ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
dba547cb-33f6-4a0e-9a35-fe368b5be4f5	\N	REQUEST_START	\N	\N	2025-08-05 19:47:43.714218-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7fbca13a-a99a-46a8-8e04-971effa08656	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:47:44.373373-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 614ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
83cc7e31-1a86-4d13-8540-a8dd23a5fde1	\N	REQUEST_START	\N	\N	2025-08-05 19:47:44.408125-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d123b778-ed36-4a9e-ac2f-9770be392783	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:47:44.625881-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 174ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f15cf93d-9990-4c14-9525-1c6b263a0e98	\N	REQUEST_START	\N	\N	2025-08-05 19:47:57.411897-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f38725e2-c710-4c83-a348-e236e55094ee	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:47:57.527526-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 114ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0d69d14a-85d0-4868-813b-579bb47a501b	\N	REQUEST_START	\N	\N	2025-08-05 19:47:57.617438-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetCompanies	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
cb994ca2-2da9-4f23-8e42-d5478874a188	\N	REQUEST_START	\N	\N	2025-08-05 19:47:57.638481-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
cec824a9-c718-4470-a85c-cfd09c4d1be5	\N	REQUEST_START	\N	\N	2025-08-05 19:47:57.629929-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
46cd1ee8-1159-4542-a8cb-ed261046e010	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:48:22.950592-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetCompanies - Status: 200 - Tiempo: 25335ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1a01fb61-7710-40b6-9b92-1bbdf9050fcf	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:48:23.027213-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 25426ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fd038637-3ab7-4822-9d50-d0805027ac30	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:48:23.066382-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 25461ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3cde2bbb-3fa0-4d01-969c-0a793f116c32	\N	REQUEST_START	\N	\N	2025-08-05 19:48:30.380016-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
90fc7383-1d59-4929-993a-939fbfc486b7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:48:30.444434-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 43ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3b9a7840-354b-44a6-9c7c-ac984d0480f8	\N	REQUEST_START	\N	\N	2025-08-05 19:48:47.241393-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a10096e5-6009-4062-9d30-4115905ea168	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:48:47.310579-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 58ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
77cf280d-7f91-41b1-94bd-89589fd2c9f4	\N	REQUEST_START	\N	\N	2025-08-05 19:48:51.638994-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetCompanies	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
93eb6bfd-34ed-469b-a842-aedc7f6b6521	\N	REQUEST_START	\N	\N	2025-08-05 19:48:51.646384-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8a445c03-b2ef-469c-8dfe-4f187524b83e	\N	REQUEST_START	\N	\N	2025-08-05 19:48:51.686183-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
135ecab7-255c-450f-b32c-c6749aa44ba8	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:48:51.764042-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetCompanies - Status: 200 - Tiempo: 133ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c3dc0b3c-5ad9-4236-aff3-abedf14545f1	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:48:51.784063-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 134ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a6ee6bf8-ee4f-4807-9ade-68ab74441453	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:48:51.829959-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 200ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
685a48c3-e414-4f91-8621-adc34e312851	\N	REQUEST_START	\N	\N	2025-08-05 19:51:29.976241-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1d35627c-78ca-484a-909c-f707e94ab3f2	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:51:31.744912-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 1954ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1f361e37-59e1-4ae8-9197-17e15464ef14	\N	REQUEST_START	\N	\N	2025-08-05 19:51:37.303949-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
07328f2c-ad40-4e3f-bcdb-6eed97557c73	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 19:51:37.955178-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 612ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8309bb5a-ec70-4158-b67b-782284edbc71	\N	REQUEST_START	\N	\N	2025-08-05 19:51:37.993605-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a7343e7b-de74-4c5c-a152-6c47cb706118	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:51:38.198084-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 173ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4fc66778-c78f-481c-b2ac-80e43bee0e17	\N	REQUEST_START	\N	\N	2025-08-05 19:51:44.554022-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9c154490-c6e3-4440-ba14-a5e57e21f722	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:51:44.669024-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 108ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f8844a17-6967-49f3-b527-73cdee94c704	\N	REQUEST_START	\N	\N	2025-08-05 19:51:44.750464-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetCompanies	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
70a7d4d9-d9c8-4d23-9922-b0d69f695d89	\N	REQUEST_START	\N	\N	2025-08-05 19:51:44.750473-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7a298f1e-bf13-465b-874a-dcd0d060794c	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:51:44.910242-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetCompanies - Status: 200 - Tiempo: 163ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0124e2b8-3e9b-4d5c-930f-703c644ca594	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:51:45.009054-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 270ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
30e39232-dd98-4c77-8617-dafc4edcc94e	\N	REQUEST_START	\N	\N	2025-08-05 19:51:51.305477-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e7b707b9-c1d2-49a8-839b-1c60f714baf1	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:51:51.391489-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 59ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
feaae837-7a85-432d-ba8b-004ffd5772d1	\N	REQUEST_START	\N	\N	2025-08-05 19:51:55.260088-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
248e0c83-3542-4178-8171-8cdb337c0309	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:51:55.314976-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 46ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2aa781b6-25ce-4714-99e0-a17a8c7a8abe	\N	REQUEST_START	\N	\N	2025-08-05 19:52:13.409868-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8254d4cd-ad34-4156-ab87-f84d1b522f2b	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:52:13.476194-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 47ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
14ee3271-c7c0-4729-b530-0d86e5c4c22b	\N	REQUEST_START	\N	\N	2025-08-05 19:52:32.842295-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
93c88630-2e8d-414f-8ace-3b2974ea9cc3	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:52:32.90469-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 46ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
00d74dc2-64aa-4f2a-abb3-c00bbd16ee7e	\N	REQUEST_START	\N	\N	2025-08-05 19:53:00.61977-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e51e436d-d530-487a-bdf5-63b6c4fb6058	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:53:00.684011-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 73ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
60c97ab5-61ec-43eb-8dc8-915990a52c6f	\N	REQUEST_START	\N	\N	2025-08-05 19:53:21.174609-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e4affafa-81d3-4556-b3ac-e42b80a6707c	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:53:21.30715-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 122ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3f12c337-d164-4bdb-9de9-4548871697c9	\N	REQUEST_START	\N	\N	2025-08-05 19:53:21.329577-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
bdc50098-9b29-49e9-a92a-bc97c4fdfc73	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:53:21.374005-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 31ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1242a92e-bd35-4ff4-9a10-0b0fa31b8782	\N	REQUEST_START	\N	\N	2025-08-05 19:54:06.134825-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /User/Update	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1b603631-010b-4576-ab75-892ebba3e688	770e8400-e29b-41d4-a716-446655440001	UPDATE	users	770e8400-e29b-41d4-a716-446655440003	2025-08-05 19:54:06.294362-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	USER	Usuario actualizado: Admin Café Express (admin@cafeexpress.com)	{\r\n  "FullName": "Admin Caf\\u00E9 Express",\r\n  "Email": "admin@cafeexpress.com",\r\n  "Role": 0,\r\n  "BranchId": "660e8400-e29b-41d4-a716-446655440004",\r\n  "IsActive": true\r\n}	{\r\n  "FullName": "Admin Caf\\u00E9 Express",\r\n  "Email": "admin@cafeexpress.com",\r\n  "Role": 0,\r\n  "BranchId": "660e8400-e29b-41d4-a716-446655440004",\r\n  "IsActive": false\r\n}	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	fec4ed4a-edca-2f41-52ff-63696ab1607f	f	\N	\N	\N	\N	\N	\N	\N
12482cd3-37b1-4c9a-a8fb-54a5ac8d5e16	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:54:06.335146-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: POST /User/Update - Status: 200 - Tiempo: 190ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
24d3b5eb-833f-4024-88f2-b27b39f93f25	\N	REQUEST_START	\N	\N	2025-08-05 19:54:06.37095-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
62c4c599-76ac-463f-b954-1ac9dc766828	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 19:54:06.425126-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 43ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d43aba15-8dc8-490a-8851-1daea88e0a37	\N	REQUEST_START	\N	\N	2025-08-05 20:33:30.285035-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c7c82c33-4352-4a38-ba22-1bce4aa5bae8	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:33:30.401377-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 107ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
acefb036-333a-4102-bfbe-3251a2dbe88c	\N	REQUEST_START	\N	\N	2025-08-05 20:33:30.426813-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d6947429-c339-405f-b77f-c091e4d74d2e	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:33:30.477757-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 36ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
856b5669-cf16-456e-b720-603d74e1409a	\N	REQUEST_START	\N	\N	2025-08-05 20:33:43.516166-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
600b83ab-8896-4d3f-83b7-84dd9a31a7f6	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:33:43.570254-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 45ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
391126da-9a2e-40cf-9488-d73445d9ce98	\N	REQUEST_START	\N	\N	2025-08-05 20:39:45.109648-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
58b7ddd8-fbce-4913-b4ec-92a66d424510	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 20:39:46.935617-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 1982ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9a836473-149c-4437-98f4-f7deccefc31b	\N	REQUEST_START	\N	\N	2025-08-05 20:39:57.642755-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
076dc73b-639d-459a-94b2-01bf8d1b191c	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 20:39:58.286192-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 657ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e02b6e0c-f167-4d1b-bf34-944f0bed40e7	\N	REQUEST_START	\N	\N	2025-08-05 20:40:03.697323-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5b8ab035-9758-42b1-bc7d-14c991d73626	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:03.81582-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 75ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b35128df-acc4-4de0-9984-ccc1bf1dc218	\N	REQUEST_START	\N	\N	2025-08-05 20:40:03.852737-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6533f4e3-6df4-4bee-8ffe-5689bfef7348	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:04.092547-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 202ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2ff3b79c-8c26-45aa-a188-f84a62c9ca80	\N	REQUEST_START	\N	\N	2025-08-05 20:40:11.444961-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
48879864-a7c5-420a-8ab3-a01d1c6b8ac7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:11.515307-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 55ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fdeaab1f-cb5d-40bf-ab09-3c7f06b85ced	\N	REQUEST_START	\N	\N	2025-08-05 20:40:18.087361-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5ddb0939-1c84-46b4-95e6-b82cfb46589e	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:18.204192-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 100ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3afa0a4b-8d88-49c2-95de-7cb9f19edf78	\N	REQUEST_START	\N	\N	2025-08-05 20:40:18.302742-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetCompanies	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6e025a89-6c4d-4db1-9ee4-9aa032dce877	\N	REQUEST_START	\N	\N	2025-08-05 20:40:18.302726-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
807acd2c-b019-43b1-b372-0140c3169865	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:18.457315-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetCompanies - Status: 200 - Tiempo: 177ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
20fc7cec-b63f-43f5-a96c-1ddbb4787565	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:18.561978-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 282ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1d3eb77b-7e3b-46d6-a230-53dbb729aac4	\N	REQUEST_START	\N	\N	2025-08-05 20:40:24.827249-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f6c16884-bb6c-4de4-aba6-cc7a17efc288	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:24.973097-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 134ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c3f34af8-262e-4774-97f2-9759224d64f8	\N	REQUEST_START	\N	\N	2025-08-05 20:40:25.000884-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
084d30ef-848b-4a97-82ee-5af1aaf3566c	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:25.057958-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 50ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7ee4cca0-f5ff-4bd0-a677-d90eb4c98fe2	\N	REQUEST_START	\N	\N	2025-08-05 20:40:25.085384-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
45d68c23-02b7-48cc-a578-d83fab924939	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:25.135864-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 40ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a16863c6-fe1e-4c57-b94f-13e95cd78b6c	\N	REQUEST_START	\N	\N	2025-08-05 20:40:32.143619-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2d62a984-6f14-425c-ac3d-46ced6dccb97	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:32.200845-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 51ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
836d5503-c2c4-4018-a789-7a0974540697	\N	REQUEST_START	\N	\N	2025-08-05 20:40:35.119074-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
573baa6e-e6a6-4195-9bae-b14059d5e354	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:35.165219-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 38ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
15eb3ec6-2d3c-4d61-b0af-7a9c964f70c0	\N	REQUEST_START	\N	\N	2025-08-05 20:40:47.062953-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e316ebb9-e025-4572-94f0-e98efb1fe0bd	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:47.113477-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 42ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a0977387-f073-4b62-b5f8-b36da5f4633f	\N	REQUEST_START	\N	\N	2025-08-05 20:41:56.89797-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ff98d0e1-3897-43bd-b8dc-d9d9e1515aa9	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:41:56.944269-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 40ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a296b667-a0c4-444b-85a5-8a756225782a	\N	REQUEST_START	\N	\N	2025-08-05 20:40:31.902147-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /User/Update	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
69709490-7d30-46dd-ad34-fd6b2dffa959	770e8400-e29b-41d4-a716-446655440001	UPDATE	users	770e8400-e29b-41d4-a716-446655440003	2025-08-05 20:40:32.063718-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	USER	Usuario actualizado: Admin Café Express (admin@cafeexpress.com)	{\r\n  "FullName": "Admin Caf\\u00E9 Express",\r\n  "Email": "admin@cafeexpress.com",\r\n  "Role": 0,\r\n  "BranchId": "660e8400-e29b-41d4-a716-446655440004",\r\n  "IsActive": false\r\n}	{\r\n  "FullName": "Admin Caf\\u00E9 Express",\r\n  "Email": "admin@cafeexpress.com",\r\n  "Role": 0,\r\n  "BranchId": "660e8400-e29b-41d4-a716-446655440004",\r\n  "IsActive": true\r\n}	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	e1bbc24f-9a23-85a4-a66f-5b439e78ef53	f	\N	\N	\N	\N	\N	\N	\N
4b68fb01-8344-4ab9-bc07-a69234c805f7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:32.104434-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: POST /User/Update - Status: 200 - Tiempo: 191ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d5d0c034-5aa2-4da6-a64d-5ede910b057c	\N	REQUEST_START	\N	\N	2025-08-05 20:40:35.017283-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
60655cef-5dcf-4a1f-95f6-ff63ef46ba76	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:35.092528-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 66ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
75627e4d-552d-4391-9035-72fb6d76a8ab	\N	REQUEST_START	\N	\N	2025-08-05 20:40:35.19192-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
070d7e1c-1456-4894-9f60-b1e10c1f8eb4	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:40:35.242621-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 40ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
28ea93ab-ca2b-4dc7-a17f-d9fe4bcacde6	\N	REQUEST_START	\N	\N	2025-08-05 20:41:54.433279-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ecdae605-cf7c-4682-b4e6-3597a0a695f2	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:41:54.494552-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 44ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c81d88d8-2fab-4369-ae23-0e4cde195f54	\N	REQUEST_START	\N	\N	2025-08-05 20:47:58.201567-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
626a62f3-31d1-4c3d-85f0-e332ab2434c3	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:00.110897-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2064ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4481efcd-9557-477c-bdf3-7cb457640f7d	\N	REQUEST_START	\N	\N	2025-08-05 20:48:06.135187-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a92aee88-2e13-495c-b7f7-4a6f7b174934	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:06.755691-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 570ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
820101f5-4deb-4d12-908e-4a9b0679415c	\N	REQUEST_START	\N	\N	2025-08-05 20:48:06.791374-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ffac4037-8ca1-4824-9d0f-31817151165f	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:06.976598-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 154ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0c877802-dd6a-4898-a558-4e3c791d7d21	\N	REQUEST_START	\N	\N	2025-08-05 20:48:13.147033-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
36e06e8d-4350-4d26-942a-b2e0ff3248be	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:13.235651-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 92ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b2189fb5-ca45-4afb-a632-892ec5ea3b3c	\N	REQUEST_START	\N	\N	2025-08-05 20:48:13.343385-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
64ad3e72-b51a-427d-8c06-9311e4f61d4a	\N	REQUEST_START	\N	\N	2025-08-05 20:48:13.343391-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetCompanies	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
89ee9434-facd-45a4-9aae-22d6ad4866eb	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:13.541854-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetCompanies - Status: 200 - Tiempo: 218ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0459c08d-b614-4ca8-abb0-e61766f14fba	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:13.583887-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 263ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8b8ee71f-eabd-45a5-841e-22adba939424	\N	REQUEST_START	\N	\N	2025-08-05 20:48:21.890011-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9816ab07-40c3-497f-a955-621b3cdd0ae4	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:21.970101-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 53ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
012c1352-677d-4a74-a884-7a2d3790c4bc	\N	REQUEST_START	\N	\N	2025-08-05 20:48:31.57389-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a8649bbb-f87f-4110-a27b-3fedea61646d	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:31.668723-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 86ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fb77424c-6ebd-44fc-8e9c-accb438ca2ab	\N	REQUEST_START	\N	\N	2025-08-05 20:48:45.387347-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: DELETE /Company/Delete	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c86b661b-5f95-44bc-9b1f-9d4f862dc482	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 20:48:45.557459-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: DELETE /Company/Delete - Status: 200 - Tiempo: 164ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
20fcca2f-480c-4375-808d-897fb434ccf4	\N	REQUEST_START	\N	\N	2025-08-05 20:51:04.968284-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0c85cf4f-efaf-4696-aab7-d2da2a62c8c0	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 20:51:05.458689-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error en solicitud: POST /Company/Create - Tiempo: 449ms	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "Cannot write DateTime with Kind=UTC to PostgreSQL type \\u0027timestamp without time zone\\u0027, consider using \\u0027timestamp with time zone\\u0027. Note that it\\u0027s not possible to mix DateTimes with different Kinds in an array, range, or multirange. (Parameter \\u0027value\\u0027)"\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48	\N	\N	\N	\N
87dd224a-f121-4f6d-8f86-0af31c6fab1a	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 20:51:05.580894-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error HTTP 500: POST /Company/Create	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "Cannot write DateTime with Kind=UTC to PostgreSQL type \\u0027timestamp without time zone\\u0027, consider using \\u0027timestamp with time zone\\u0027. Note that it\\u0027s not possible to mix DateTimes with different Kinds in an array, range, or multirange. (Parameter \\u0027value\\u0027)"\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 78\r\n   at RestBar.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 168	\N	\N	\N	\N
3e077539-4227-41bb-81fb-a0b2121661ab	\N	REQUEST_START	\N	\N	2025-08-05 16:18:59.80078-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
145374e4-d5dd-431d-8993-a5d89701a6d6	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:02.136549-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2494ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
700a54b7-ad91-420b-bb13-3265139efcaa	\N	REQUEST_START	\N	\N	2025-08-05 16:19:11.594281-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1f06a0ba-eb4e-4488-b072-12065e80e2a8	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:12.289034-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 701ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c2c3cf42-9e4f-4e35-8e17-d4b5dd88dde8	\N	REQUEST_START	\N	\N	2025-08-05 16:19:12.397266-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
184b415c-35b8-4772-b90d-7771d5b661da	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:12.638331-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 213ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c5ddd05c-ef1f-436b-a1c3-695a442a1c14	\N	REQUEST_START	\N	\N	2025-08-05 16:19:19.924666-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/UserManagement	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d43afec0-b8d4-4c4c-a972-ca6267a0b483	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:20.058933-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/UserManagement - Status: 200 - Tiempo: 123ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
81c6a913-dbde-4487-aee3-60273f0cd069	\N	REQUEST_START	\N	\N	2025-08-05 16:19:20.144509-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetCompanies	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a348cbd5-5925-4ae8-b20d-825334d6a3a1	\N	REQUEST_START	\N	\N	2025-08-05 16:19:20.14451-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUsers	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fc76bf2d-5e10-47e8-87b0-473a0edcce3d	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:20.357586-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetCompanies - Status: 200 - Tiempo: 206ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d93b17ec-c9da-49f9-bdf8-278dc450f35a	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:20.473401-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUsers - Status: 200 - Tiempo: 323ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9129a7e7-9392-4d61-82cb-8ee69f0889b9	\N	REQUEST_START	\N	\N	2025-08-05 16:19:24.837458-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetUser	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7b04dc4d-d0b3-4849-a3a1-5b66887eed7b	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:24.982386-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetUser - Status: 200 - Tiempo: 139ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1910ba8f-5bae-4025-a676-151cfa67eda0	\N	REQUEST_START	\N	\N	2025-08-05 16:19:25.037507-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ea00ca8a-fd7f-4518-9ce0-6e73dc6ef70c	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:25.105655-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 62ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d7b10e6b-b310-4d19-9421-ce53412d1f18	\N	REQUEST_START	\N	\N	2025-08-05 16:19:29.737483-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6e9fa63f-8b53-4013-b9b0-b7cd628380a6	\N	REQUEST_START	\N	\N	2025-08-05 16:19:36.296679-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
27474141-6f95-4aed-8dd9-5f8576305534	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:36.390502-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 87ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4fc5602b-64ed-4c69-b66b-e6a194b878ab	\N	REQUEST_START	\N	\N	2025-08-05 16:19:25.145025-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /User/GetBranches	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
35d33841-d196-4ce3-98a6-55389f0f7f19	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:25.193901-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /User/GetBranches - Status: 200 - Tiempo: 43ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
42dc77a0-3bd5-4c3a-b173-70905e9edab4	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:19:29.811322-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 69ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7acc3d52-bb85-4e51-9ae0-3ffdd5faec5e	\N	REQUEST_START	\N	\N	2025-08-05 16:21:13.639215-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a8abcccf-8e4b-4798-9aa8-346e98b9628f	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:21:13.791508-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: POST /Company/Create - Status: 200 - Tiempo: 150ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d5d0b9ec-4777-4b88-944d-b01382731a4f	\N	REQUEST_START	\N	\N	2025-08-05 16:21:24.4602-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1a3be35f-1f17-4c4d-81c0-cdd0b3be2774	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:21:26.259412-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 1945ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
547fb71e-a44c-4d6f-9914-69969b394931	\N	REQUEST_START	\N	\N	2025-08-05 16:21:33.0693-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
6f52d822-8c8d-4e0d-8477-6b7b4605a0ad	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:21:33.64121-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 576ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9ba10cae-92d4-44e0-bc11-4e59f9c32d2a	\N	REQUEST_START	\N	\N	2025-08-05 16:21:33.731473-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f2f0212f-a945-45cf-af34-a6aca514c764	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:21:33.904137-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 157ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c23476c9-007c-40ef-9351-43f181f7ca59	\N	REQUEST_START	\N	\N	2025-08-05 16:21:44.925764-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
33c188de-69ca-46e3-8fb4-71fa6b3d5304	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:21:45.08125-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 145ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
101f2bfa-01c1-4b00-a8e8-86595b933f69	\N	REQUEST_START	\N	\N	2025-08-05 16:21:56.388746-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5436ce20-36d4-4b53-b6d1-ecbc2717a1e5	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:21:56.958293-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error en solicitud: POST /Company/Create - Tiempo: 559ms	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48	\N	\N	\N	\N
d407a258-1b89-4c3b-bb41-2d687e9b87d9	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:21:57.126154-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error HTTP 500: POST /Company/Create	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 78\r\n   at RestBar.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 168	\N	\N	\N	\N
50548cf8-6dd0-4bf7-a8a7-2e2013b62d76	\N	REQUEST_START	\N	\N	2025-08-05 16:24:42.311272-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ac795b96-c086-47d9-b530-d49563a7e9cb	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:24:44.64321-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2515ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e18a9d69-3964-4ba0-a9a5-d8ff1857a1ad	\N	REQUEST_START	\N	\N	2025-08-05 16:24:54.097515-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
deb3043a-41ae-4be9-97d9-21f3aa01a8c7	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:24:54.705337-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 612ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4ce3752c-2366-4471-9ef3-ec113dd4c321	\N	REQUEST_START	\N	\N	2025-08-05 16:24:54.804617-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2876f576-dbf0-4cd7-bfcf-27f194fe5cb1	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:24:54.975652-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 155ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d8300caf-5032-41c3-9f58-28ccddb946bf	\N	REQUEST_START	\N	\N	2025-08-05 16:25:00.094149-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3090ad0e-5791-4e7e-9b34-603b823ebbfc	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:25:00.248406-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 145ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
275e0c2a-01da-4b1c-9dd2-6f79f34adc6e	\N	REQUEST_START	\N	\N	2025-08-05 16:25:23.553233-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
952ca170-b8b5-4805-8bc7-fe81f9414801	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:25:24.080135-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error en solicitud: POST /Company/Create - Tiempo: 519ms	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48	\N	\N	\N	\N
c8bfbb71-11c9-4757-ab25-f550af1cb9ad	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:25:24.196986-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error HTTP 500: POST /Company/Create	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 78\r\n   at RestBar.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 168	\N	\N	\N	\N
ab5ddf38-34d0-4d7f-86df-db9a3e64d9a2	\N	REQUEST_START	\N	\N	2025-08-05 16:37:12.294216-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3390266c-d54a-462c-b925-89931fafd213	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:37:14.116935-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 1994ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1737b1b7-18c7-48b1-be53-850398629b5f	\N	REQUEST_START	\N	\N	2025-08-05 16:37:23.049509-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d6e42a7d-45d9-48ba-8d54-813eec27660f	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:37:23.632831-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 587ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
15e3eded-8060-491d-a79f-e41f6bcd95ac	\N	REQUEST_START	\N	\N	2025-08-05 16:37:23.731136-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b8a6d9b4-a6a8-4cf6-aa8f-6c4bc921d910	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:37:23.908268-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 161ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
881f5217-e9b1-455f-aced-a52d581714c1	\N	REQUEST_START	\N	\N	2025-08-05 16:37:30.251938-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
169934ef-e706-479e-864f-5b2bb5f81232	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:37:30.398771-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 137ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4f3c38ae-54ef-4ecf-8fa6-750df89a7933	\N	REQUEST_START	\N	\N	2025-08-05 16:37:43.60684-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
00058e69-46b1-4d63-97d0-b72570f2f5da	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:37:44.117639-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error en solicitud: POST /Company/Create - Tiempo: 505ms	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48	\N	\N	\N	\N
e2a93656-e05d-4bd8-8bbb-3a41a256b601	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:37:44.246361-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error HTTP 500: POST /Company/Create	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 78\r\n   at RestBar.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 168	\N	\N	\N	\N
dc3f3823-ff6c-496f-82ee-bf8060df317a	\N	REQUEST_START	\N	\N	2025-08-05 16:37:58.907919-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4a68c29a-3d89-46b5-82fb-31923a9e2250	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:38:00.711263-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 1939ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f97ecc56-cee4-4309-90d1-4028baaa6d37	\N	REQUEST_START	\N	\N	2025-08-05 16:38:16.331494-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
62830143-a209-4ef7-9b9f-a9ea4dc6c878	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:38:16.923703-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 598ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5b445c74-a159-4c0c-852c-5abc5a3033e2	\N	REQUEST_START	\N	\N	2025-08-05 16:38:17.019281-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d5776ca2-cf3b-45d5-a2ed-b1ed346a1135	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:38:17.197032-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 150ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e00efad1-ffb2-4da4-b365-a45ebe085727	\N	REQUEST_START	\N	\N	2025-08-05 16:38:27.02846-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2d6ce7a3-dea4-4c75-8120-4282bdb5ae32	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:38:27.148817-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 113ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9f776abc-8f1c-40cd-8bb7-9fa17a709ae3	\N	REQUEST_START	\N	\N	2025-08-05 16:38:37.129219-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
051771d5-706a-498a-aa06-1db538956363	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:38:37.668062-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error en solicitud: POST /Company/Create - Tiempo: 531ms	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48	\N	\N	\N	\N
a1f63293-53a6-4ac2-9f79-b362ebc3aa3b	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:38:37.867576-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error HTTP 500: POST /Company/Create	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 32\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 78\r\n   at RestBar.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 168	\N	\N	\N	\N
86d87070-7e41-4793-a6f7-d057bc2946c9	\N	REQUEST_START	\N	\N	2025-08-05 16:44:05.685142-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b800f0f5-f92b-420b-b9c7-82313e4ce45e	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:44:07.486155-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 1986ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fe8aee12-763c-4247-aff9-bfdcce4c5cbb	\N	REQUEST_START	\N	\N	2025-08-05 16:44:14.221239-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
68be158c-a2d1-43ff-8e7d-fe650dbfc13a	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:44:14.888976-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 672ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9794176a-1aec-48b6-b0d9-2ed0810f6e7e	\N	REQUEST_START	\N	\N	2025-08-05 16:44:14.986974-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5ba3bb5d-08f5-4247-aad4-53767c4d392b	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:44:15.192215-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 187ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4230c8f5-43a9-4498-9915-5147e82ce96e	\N	REQUEST_START	\N	\N	2025-08-05 16:44:20.773538-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b1f2a74d-4085-4b8a-9674-38751cbfc537	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:44:20.961972-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 178ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a9b6f9a6-8ec0-4fdd-b609-706e563c38a7	\N	REQUEST_START	\N	\N	2025-08-05 16:44:31.506914-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0c231f02-486c-4c83-8817-0003b3fedbf3	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:57:44.47392-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 132ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ee935537-9afd-49b7-8b92-1ae978f0d316	\N	REQUEST_START	\N	\N	2025-08-05 16:57:57.39208-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f78ef61a-c7aa-4ca8-bd78-21ee8fa0145f	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:21:43.840617-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2046ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
a4405cb3-b6b0-4ccb-ba4b-e79485d93411	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:44:32.058087-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error en solicitud: POST /Company/Create - Tiempo: 544ms	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 37\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48	\N	\N	\N	\N
075a5105-8132-4c10-a90b-2c5aab68e5a4	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:44:32.226323-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error HTTP 500: POST /Company/Create	\N	\N	{\r\n  "Message": "An error occurred while saving the entity changes. See the inner exception for details.",\r\n  "Source": "Microsoft.EntityFrameworkCore.Relational",\r\n  "InnerException": "23502: el valor nulo en la columna \\u00ABIsActive\\u00BB de la relaci\\u00F3n \\u00ABcompanies\\u00BB viola la restricci\\u00F3n de no nulo\\r\\n\\r\\nDETAIL: Detail redacted as it may contain sensitive data. Specify \\u0027Include Error Detail\\u0027 in the connection string to include this information."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	DbUpdateException	   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)\r\n   at RestBar.Models.RestBarContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\\RestBar\\RestBar\\Models\\RestBarContext.cs:line 1207\r\n   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 37\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 78\r\n   at RestBar.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 168	\N	\N	\N	\N
cb219b96-ef74-4cb5-981c-825fadc05bf7	\N	REQUEST_START	\N	\N	2025-08-05 16:47:37.020717-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
45cd9d79-12fd-4239-b177-cc3ce945b771	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:47:38.885364-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2002ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
bb73182f-662a-439a-841b-b21707e43fbd	\N	REQUEST_START	\N	\N	2025-08-05 16:47:47.291935-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ab7365ce-dc67-4cbc-b53d-c0181edc0384	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:47:47.889252-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 602ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
77dca064-26b9-43cc-b83d-1d01dc98962d	\N	REQUEST_START	\N	\N	2025-08-05 16:47:47.978851-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
eb76af72-4b7c-4e13-8332-1fdd3ff17858	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:47:48.163372-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 168ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0d74711e-3f31-4a05-9b64-da715d471fee	\N	REQUEST_START	\N	\N	2025-08-05 16:50:49.064591-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c1e5064b-a819-40b4-83f8-1399d4bddee7	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:50:49.218078-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 145ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
13414ac6-4653-4c4a-a089-ec8076c66981	\N	REQUEST_START	\N	\N	2025-08-05 16:50:58.269262-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
43d68eed-821c-4dc3-81b5-4416fd8c271b	\N	REQUEST_START	\N	\N	2025-08-05 16:57:29.255568-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
51fc4fe1-da3b-4490-9dc3-100e4a87c309	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:57:31.227316-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2170ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d439e0dc-4b21-4cbe-80be-59633afd52c2	\N	REQUEST_START	\N	\N	2025-08-05 16:57:37.848344-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
f48b7749-44ce-40c1-8a9f-f44395e069ca	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 16:57:38.487666-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 646ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
117e9945-cd74-4fa3-b8ea-3b90c1ca25f2	\N	REQUEST_START	\N	\N	2025-08-05 16:57:38.61019-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c6def065-9676-417d-8ad6-e7a54ca398de	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 16:57:38.839801-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 209ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3216080c-dee8-44ec-ae03-056bc8e45c71	\N	REQUEST_START	\N	\N	2025-08-05 16:57:44.330986-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0fd0d203-d341-44e0-abc2-a73b5cb22f52	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:58:07.669737-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error en solicitud: POST /Company/Create - Tiempo: 10270ms	\N	\N	{\r\n  "Message": "Ocurri\\u00F3 un error al guardar la compa\\u00F1\\u00EDa en la base de datos.",\r\n  "Source": "RestBar",\r\n  "InnerException": "An error occurred while saving the entity changes. See the inner exception for details."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	ApplicationException	   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 62\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48	\N	\N	\N	\N
e13cf9b3-0361-4cec-8803-7c359d9bd28c	770e8400-e29b-41d4-a716-446655440001	ERROR	\N	\N	2025-08-05 16:58:07.809679-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	ERROR	SYSTEM	Error HTTP 500: POST /Company/Create	\N	\N	{\r\n  "Message": "Ocurri\\u00F3 un error al guardar la compa\\u00F1\\u00EDa en la base de datos.",\r\n  "Source": "RestBar",\r\n  "InnerException": "An error occurred while saving the entity changes. See the inner exception for details."\r\n}	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	t	\N	ApplicationException	   at RestBar.Services.CompanyService.CreateAsync(Company company) in C:\\RestBar\\RestBar\\Services\\CompanyService.cs:line 62\r\n   at RestBar.Controllers.CompanyController.Create(Company model) in C:\\RestBar\\RestBar\\Controllers\\CompanyController.cs:line 95\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\r\n   at RestBar.Middleware.PermissionMiddleware.InvokeAsync(HttpContext context, IAuthService authService) in C:\\RestBar\\RestBar\\Middleware\\PermissionMiddleware.cs:line 74\r\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at Microsoft.AspNetCore.Session.SessionMiddleware.Invoke(HttpContext context)\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 48\r\n   at RestBar.Middleware.AuditMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 78\r\n   at RestBar.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in C:\\RestBar\\RestBar\\Middleware\\AuditMiddleware.cs:line 168	\N	\N	\N	\N
9e94c671-eecb-4847-93a0-7e97acf119d8	\N	REQUEST_START	\N	\N	2025-08-05 16:58:11.509105-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
467da6ba-ea2d-4a58-bb22-d880fc1e2d38	\N	REQUEST_START	\N	\N	2025-08-05 17:04:51.659388-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
5ce227fb-630b-4558-8c45-9e371e111439	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:04:53.698832-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2244ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e658643f-19a7-4f1b-95e5-70b700ad53bc	\N	REQUEST_START	\N	\N	2025-08-05 17:04:59.621054-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3eae6e78-91a0-427d-afc7-ca541e004dbe	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:05:00.346111-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 302 - Tiempo: 731ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e9dcdd7c-7d57-4de9-a75b-9398fefcfb28	\N	REQUEST_START	\N	\N	2025-08-05 17:05:00.461409-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Home/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2898c6bd-9690-4f9a-8a55-1672fdd020f3	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 17:05:00.675124-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Home/Index - Status: 200 - Tiempo: 194ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ccb9b1ae-b352-4c72-b8bd-aa053bb9249d	\N	REQUEST_START	\N	\N	2025-08-05 17:05:07.067808-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /Company/Index	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
deefeeb3-7ef6-4f1a-a895-d2f63874502f	770e8400-e29b-41d4-a716-446655440001	REQUEST_SUCCESS	\N	\N	2025-08-05 17:05:07.215869-05	\N	\N	\N	\N	\N	\N	550e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	INFO	SYSTEM	Solicitud completada: GET /Company/Index - Status: 200 - Tiempo: 138ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2c7b5e78-79b8-403e-ba12-0d841b4a8ee6	\N	REQUEST_START	\N	\N	2025-08-05 17:05:21.438006-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /Company/Create	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8111ee2b-df8c-4468-a860-33247982997f	\N	REQUEST_START	\N	\N	2025-08-05 17:20:36.091554-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
071b2aa3-8990-4efe-92e1-6d68c9a9478a	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:20:38.19868-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2328ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
54605661-7279-4136-9f7c-50cf1e5cf53b	\N	REQUEST_START	\N	\N	2025-08-05 17:20:44.743821-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
64a6b062-c448-4564-9b70-8fb1361d154c	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:20:45.387426-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 649ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
03618d2e-e0cb-45c5-99fa-edfafa895d38	\N	REQUEST_START	\N	\N	2025-08-05 17:20:50.924615-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
38a32a9e-c672-4c91-929e-b8526123e430	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:20:51.051721-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 128ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e82f38aa-3d41-4ff4-be35-50381b2821e4	\N	REQUEST_START	\N	\N	2025-08-05 17:20:56.281572-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b5700779-2238-44df-a0f7-8186dd896f8c	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:20:56.38239-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 101ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
159c5f5f-6f2c-49c8-b1ad-7d6bfea6e7eb	\N	REQUEST_START	\N	\N	2025-08-05 17:21:10.416824-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
e0957cd4-be01-456c-b511-9a970b312737	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:21:10.507641-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 91ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c65fee6e-9e2f-404d-8fc0-fbd535d0842c	\N	REQUEST_START	\N	\N	2025-08-05 17:21:41.9448-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
57ca6ebd-2d75-4540-bf7e-df41ebf18d4c	\N	REQUEST_START	\N	\N	2025-08-05 17:21:55.192896-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
fe0c7d0e-7883-47b1-9f5c-8f7b6b967224	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:21:57.128295-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2097ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
ebb5ae4a-077f-4796-887d-fb508413121c	\N	REQUEST_START	\N	\N	2025-08-05 17:22:03.720275-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
0df7debf-4283-4206-9897-e80c708aee85	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:22:04.26553-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 549ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
b4073fc8-30fb-4756-bdb1-cabd43e5d37f	\N	REQUEST_START	\N	\N	2025-08-05 17:22:11.512866-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
4b4ff645-36d5-4a6c-bdbe-9fd0feb49afb	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:22:11.619917-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 108ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
382993ff-8795-46b2-815d-d61f50cf5244	\N	REQUEST_START	\N	\N	2025-08-05 17:24:10.354479-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
9b30605b-2796-4d4c-8b13-5d5458fe6162	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:24:12.401304-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2208ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
1520aa3e-db99-4c50-bf65-e45d68697692	\N	REQUEST_START	\N	\N	2025-08-05 17:24:18.026652-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
59089429-c4ba-4b0c-8639-bfd44f3f53a9	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:24:18.59464-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 572ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
50663e48-6140-4e5f-b7c2-789f11726d26	\N	REQUEST_START	\N	\N	2025-08-05 17:24:53.793981-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
efb1f197-0099-40e3-803c-0b96e4800b56	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:24:53.905708-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 113ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
d1134462-1b3a-4989-a75c-5adb1c7e32fc	\N	REQUEST_START	\N	\N	2025-08-05 17:25:08.230493-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
29a74509-cb57-49c8-b39c-bdf669a96c79	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:25:10.215521-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2168ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
afd3da91-140e-4eb9-8bcb-d5855c2f6c2e	\N	REQUEST_START	\N	\N	2025-08-05 17:25:19.976566-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
c9ff9935-a54f-444e-9e21-03e9ad076200	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:25:20.539521-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 571ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
03d89e58-1ff2-4847-9b94-6e9b7216b1bf	\N	REQUEST_START	\N	\N	2025-08-05 17:29:59.011981-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: GET /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
8d135067-0208-4a03-bb3f-23f730ca82cc	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:30:00.938877-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: GET / - Status: 200 - Tiempo: 2096ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
09dda777-a329-40c0-a288-81b01cf62f48	\N	REQUEST_START	\N	\N	2025-08-05 17:30:06.305696-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
7327f9a7-1adb-4077-98ae-e202789e5548	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:30:06.908686-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 609ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2812c257-45b9-458c-ae69-bad4280c6993	\N	REQUEST_START	\N	\N	2025-08-05 17:30:35.321481-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3ea00990-1cc4-488a-be69-a0cdd1e302bd	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:30:35.432638-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 112ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
2dbb8de7-be2f-44b2-a148-f5dd2d665c89	\N	REQUEST_START	\N	\N	2025-08-05 17:31:24.885016-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
abfbbd72-02c1-42b4-83a9-27c9ce48da1e	\N	REQUEST_SUCCESS	\N	\N	2025-08-05 17:32:09.278329-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Solicitud completada: POST / - Status: 200 - Tiempo: 44393ms	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
3ee4f8eb-affb-4e01-ad77-388c107ce1d5	\N	REQUEST_START	\N	\N	2025-08-05 17:32:15.182942-05	\N	\N	\N	\N	\N	\N	\N	\N	INFO	SYSTEM	Inicio de solicitud: POST /	\N	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36	\N	f	\N	\N	\N	\N	\N	\N	\N
\.


--
-- TOC entry 5447 (class 0 OID 175307)
-- Dependencies: 224
-- Data for Name: branches; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.branches (id, company_id, name, address, phone, is_active, created_at, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
660e8400-e29b-41d4-a716-446655440001	550e8400-e29b-41d4-a716-446655440001	RestBar Centro	Av. Balboa 123	555-0101	t	2024-01-01 00:00:00-05	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
660e8400-e29b-41d4-a716-446655440002	550e8400-e29b-41d4-a716-446655440001	RestBar Norte	Calle Norte 456	555-0102	t	2024-01-01 00:00:00-05	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
660e8400-e29b-41d4-a716-446655440003	550e8400-e29b-41d4-a716-446655440001	RestBar Este	Av. Este 789	555-0103	t	2024-01-01 00:00:00-05	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
660e8400-e29b-41d4-a716-446655440004	550e8400-e29b-41d4-a716-446655440002	Café Express San José	Calle Central 456	555-0201	t	2024-01-01 00:00:00-05	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
660e8400-e29b-41d4-a716-446655440005	550e8400-e29b-41d4-a716-446655440002	Café Express Heredia	Av. Heredia 123	555-0202	t	2024-01-01 00:00:00-05	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
660e8400-e29b-41d4-a716-446655440006	550e8400-e29b-41d4-a716-446655440003	El Sabor Bogotá	Plaza Mayor 789	555-0301	t	2024-01-01 00:00:00-05	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
660e8400-e29b-41d4-a716-446655440007	550e8400-e29b-41d4-a716-446655440003	El Sabor Medellín	Calle Medellín 321	555-0302	t	2024-01-01 00:00:00-05	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
\.


--
-- TOC entry 5448 (class 0 OID 175322)
-- Dependencies: 225
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.categories (id, name, description, is_active, "CompanyId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
97188e72-4706-4a3b-ba1f-60d6065af86e	Bebidas	Bebidas y refrescos	t	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
50698afb-d189-4f50-97cc-84b429a36ded	Comidas	Platos principales	t	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
f05e3f58-5c05-45eb-9e3d-a85eb23faf65	Postres	Dulces y postres	t	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
b85e917c-25b5-4df9-86e4-97d12d9de0e3	Entradas	Aperitivos y entradas	t	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
\.


--
-- TOC entry 5443 (class 0 OID 175273)
-- Dependencies: 220
-- Data for Name: companies; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.companies (id, name, tax_id, address, phone, email, is_active, legal_id, created_at, updated_at, "CreatedAt", "UpdatedAt", created_by, updated_by) FROM stdin;
550e8400-e29b-41d4-a716-446655440001	RestBar Panamá	123456789	Av. Balboa 123, Panamá	+507 555-0101	info@restbar-panama.com	t	9-713-136	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
550e8400-e29b-41d4-a716-446655440002	Café Express Costa Rica	987654321	Calle Central 456, San José	+506 555-0202	info@cafeexpress-cr.com	t	8-713-136	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	2025-08-05 13:01:52.65297-05	\N	\N
550e8400-e29b-41d4-a716-446655440003	Restaurante El Sabor	456789123	Plaza Mayor 789, Bogotá	+57 555-0303	info@elsabor-col.com	t	7-85645-65452123	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	2025-08-05 13:02:06.715443-05	\N	\N
\.


--
-- TOC entry 5449 (class 0 OID 175336)
-- Dependencies: 226
-- Data for Name: company_settings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.company_settings (id, company_id, setting_key, setting_value, created_at, "CompanyId1") FROM stdin;
\.


--
-- TOC entry 5450 (class 0 OID 175355)
-- Dependencies: 227
-- Data for Name: company_subscriptions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.company_subscriptions (id, company_id, plan_type, start_date, end_date, is_active, created_at, "CompanyId1") FROM stdin;
\.


--
-- TOC entry 5444 (class 0 OID 175282)
-- Dependencies: 221
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.customers (id, full_name, email, phone, loyalty_points, notes, created_at, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5459 (class 0 OID 175541)
-- Dependencies: 236
-- Data for Name: inventory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inventory (id, branch_id, product_id, quantity, unit, min_threshold, last_updated, stock, min_stock, max_stock, company_id, expiration_date, manufacturing_date, lot_number, batch_number, is_perishable, days_until_expiration, "Barcode", "ExpiryDate", "IsActive", "Location", "Notes", "ReorderPoint", "ReorderQuantity", "TotalValue", "UnitCost", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5469 (class 0 OID 175783)
-- Dependencies: 246
-- Data for Name: inventory_histories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inventory_histories (id, inventory_id, product_id, branch_id, company_id, quantity, quantity_before, quantity_after, unit, expiration_date, manufacturing_date, lot_number, batch_number, type, reason, notes, created_by_user_id, created_at) FROM stdin;
\.


--
-- TOC entry 5475 (class 0 OID 175997)
-- Dependencies: 252
-- Data for Name: inventory_movements; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inventory_movements (id, inventory_id, product_id, branch_id, user_id, type, quantity, previous_stock, new_stock, reason, reference, created_at, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5461 (class 0 OID 175579)
-- Dependencies: 238
-- Data for Name: invoices; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.invoices (id, order_id, customer_id, total, tax, created_at, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5466 (class 0 OID 175680)
-- Dependencies: 243
-- Data for Name: journal_entries; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.journal_entries (id, entry_number, entry_date, type, description, reference, status, posted_at, posted_by, total_debit, total_credit, created_at, updated_at, created_by, updated_by, order_id, payment_id, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5468 (class 0 OID 175711)
-- Dependencies: 245
-- Data for Name: journal_entry_details; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.journal_entry_details (id, journal_entry_id, account_id, debit_amount, credit_amount, description, reference) FROM stdin;
\.


--
-- TOC entry 5445 (class 0 OID 175292)
-- Dependencies: 222
-- Data for Name: modifiers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.modifiers (id, name, extra_cost) FROM stdin;
\.


--
-- TOC entry 5462 (class 0 OID 175596)
-- Dependencies: 239
-- Data for Name: notifications; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.notifications (id, order_id, message, is_read, created_at, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5463 (class 0 OID 175611)
-- Dependencies: 240
-- Data for Name: order_cancellation_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.order_cancellation_logs (id, order_id, user_id, supervisor_id, reason, date, products) FROM stdin;
\.


--
-- TOC entry 5464 (class 0 OID 175635)
-- Dependencies: 241
-- Data for Name: order_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.order_items (id, order_id, product_id, quantity, unit_price, discount, notes, status, prepared_by_station_id, prepared_at, "KitchenStatus", "SentAt", "StationId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5458 (class 0 OID 175512)
-- Dependencies: 235
-- Data for Name: orders; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.orders (id, "OrderNumber", table_id, customer_id, user_id, order_type, status, total_amount, opened_at, closed_at, notes, company_id, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5465 (class 0 OID 175664)
-- Dependencies: 242
-- Data for Name: payments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.payments (id, order_id, method, amount, paid_at, is_voided, is_shared, payer_name, status, created_at, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5446 (class 0 OID 175299)
-- Dependencies: 223
-- Data for Name: product_categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.product_categories (id, name, description, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5460 (class 0 OID 175564)
-- Dependencies: 237
-- Data for Name: product_modifiers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.product_modifiers (product_id, modifier_id) FROM stdin;
\.


--
-- TOC entry 5456 (class 0 OID 175455)
-- Dependencies: 233
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.products (id, name, description, price, cost, tax_rate, unit, image_url, is_active, created_at, category_id, station_id, "Stock", company_id, "ProductCategoryId", is_perishable, shelf_life_days, alert_days_before_expiration, storage_conditions, requires_lot_tracking, requires_expiration_date, "SupplierId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
1d6f0675-7f05-4fbf-bf08-5fcb40d681e4	Hamburguesa ClÃ¡sica	Hamburguesa con carne, lechuga, tomate y queso	8.99	4.50	16.00	Unidad	\N	t	2025-08-04 11:52:23.477826-05	50698afb-d189-4f50-97cc-84b429a36ded	\N	\N	\N	\N	f	\N	30	\N	f	f	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
683c8473-5502-45d1-bb84-1464db7617a9	Pizza Margherita	Pizza con tomate, mozzarella y albahaca	12.50	6.25	16.00	Pizza	\N	t	2025-08-04 11:52:23.477826-05	50698afb-d189-4f50-97cc-84b429a36ded	\N	\N	\N	\N	f	\N	30	\N	f	f	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
a978f7dd-0a03-40cc-b465-783ef5a34918	TiramisÃº	Postre italiano con cafÃ© y mascarpone	6.99	3.50	16.00	PorciÃ³n	\N	t	2025-08-04 11:52:23.477826-05	f05e3f58-5c05-45eb-9e3d-a85eb23faf65	\N	\N	\N	\N	f	\N	30	\N	f	f	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
2a4819de-9bad-47d2-8206-022a6f8dd1fb	Papas Fritas	Papas fritas crujientes	4.50	2.25	16.00	PorciÃ³n	\N	t	2025-08-04 11:52:23.477826-05	b85e917c-25b5-4df9-86e4-97d12d9de0e3	\N	\N	\N	\N	f	\N	30	\N	f	f	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
4f21f408-efc5-48d5-b7e8-28708e4fb5f0	Coca Cola	Refresco de cola 350ml	2.50	1.20	16.00	Botella	\N	t	2025-08-04 11:52:23.477826-05	97188e72-4706-4a3b-ba1f-60d6065af86e	\N	\N	\N	\N	f	\N	30	\N	f	f	\N	2025-08-05 12:54:52.808252-05	2025-08-05 13:01:52.65297-05	\N	\N
\.


--
-- TOC entry 5472 (class 0 OID 175873)
-- Dependencies: 249
-- Data for Name: purchase_order_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.purchase_order_items (id, purchase_order_id, product_id, product_name, product_description, quantity, unit_price, total_price, unit, tax_rate, tax_amount, discount_rate, discount_amount, received_quantity, pending_quantity, expected_expiration_date, lot_number, notes, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5474 (class 0 OID 175925)
-- Dependencies: 251
-- Data for Name: purchase_order_receipt_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.purchase_order_receipt_items (id, purchase_order_receipt_id, purchase_order_item_id, product_id, product_name, ordered_quantity, received_quantity, accepted_quantity, rejected_quantity, unit_price, total_price, unit, expiration_date, manufacturing_date, lot_number, batch_number, quality_notes, rejection_reason, status) FROM stdin;
\.


--
-- TOC entry 5473 (class 0 OID 175893)
-- Dependencies: 250
-- Data for Name: purchase_order_receipts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.purchase_order_receipts (id, purchase_order_id, receipt_number, receipt_date, received_by_user_id, branch_id, company_id, status, notes, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 5471 (class 0 OID 175838)
-- Dependencies: 248
-- Data for Name: purchase_orders; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.purchase_orders (id, order_number, branch_id, company_id, supplier_id, created_by_user_id, order_date, expected_delivery_date, actual_delivery_date, status, total_amount, tax_amount, discount_amount, notes, created_at, updated_at, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5467 (class 0 OID 175700)
-- Dependencies: 244
-- Data for Name: split_payments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.split_payments (id, payment_id, person_name, amount, method) FROM stdin;
\.


--
-- TOC entry 5453 (class 0 OID 175406)
-- Dependencies: 230
-- Data for Name: stations; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.stations (id, name, type, icon, area_id, is_active, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5470 (class 0 OID 175823)
-- Dependencies: 247
-- Data for Name: suppliers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.suppliers (id, name, contact_person, email, phone, address, city, state, postal_code, country, tax_id, payment_terms, lead_time_days, is_active, created_at, updated_at, company_id, "Description", "ContactPerson", "Email", "Phone", "Fax", "Address", "City", "State", "PostalCode", "Country", "TaxId", "AccountNumber", "Website", "Notes", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
880e8400-e29b-41d4-a716-446655440001	Distribuidora Central S.A.	María González	maria@distribuidoracentral.com	+507-555-0101	Calle 50 #123	Panamá	Panamá	\N	Panamá	8-123-456	Neto 30 días	3	t	2025-07-21 19:12:39.251649-05	\N	550e8400-e29b-41d4-a716-446655440001	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:45:30.527131-05	2025-08-05 12:54:52.808252-05	\N	\N
880e8400-e29b-41d4-a716-446655440002	Proveedora Nacional	Carlos Rodríguez	carlos@proveedoranacional.com	+507-555-0102	Av. Balboa #456	Panamá	Panamá	\N	Panamá	8-123-457	Neto 15 días	2	t	2025-07-21 19:12:39.251649-05	\N	550e8400-e29b-41d4-a716-446655440001	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:45:30.527131-05	2025-08-05 12:54:52.808252-05	\N	\N
880e8400-e29b-41d4-a716-446655440003	Café Costa Rica S.A.	Ana Jiménez	ana@cafecostarica.com	+506-555-0201	Calle Central #789	San José	San José	\N	Costa Rica	1-234-567	Neto 30 días	1	t	2025-07-21 19:12:39.251649-05	\N	550e8400-e29b-41d4-a716-446655440002	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:45:30.527131-05	2025-08-05 12:54:52.808252-05	\N	\N
880e8400-e29b-41d4-a716-446655440004	Distribuidora Bogotá Ltda.	Luis Martínez	luis@distribuidorabogota.com	+57-555-0301	Carrera 7 #123	Bogotá	Cundinamarca	\N	Colombia	900-123-456	Neto 30 días	2	t	2025-07-21 19:12:39.251649-05	\N	550e8400-e29b-41d4-a716-446655440003	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:45:30.527131-05	2025-08-05 12:54:52.808252-05	\N	\N
b2aa3603-6951-493e-8044-689c85356744	Distribuidora Central S.A.	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:55:31.586848-05	\N	\N	Proveedor principal de productos alimenticios y bebidas	MarÃ­a GonzÃ¡lez	maria.gonzalez@distribuidoracentral.com	+507 234-5678	+507 234-5679	Calle 50, Edificio Plaza Central	PanamÃ¡	PanamÃ¡	0801	PanamÃ¡	8-123-456789	1234567890	https://www.distribuidoracentral.com	Proveedor confiable con mÃ¡s de 10 aÃ±os de experiencia	t	2025-08-04 12:55:31.586848-05	2025-08-05 12:54:52.808252-05	\N	\N
b37266d2-7bb2-442e-8ccd-dc8d14fefe02	Bebidas Premium Ltda.	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:55:31.586848-05	\N	\N	Especialistas en bebidas alcohÃ³licas y no alcohÃ³licas	Carlos RodrÃ­guez	carlos.rodriguez@bebidaspremium.com	+507 345-6789	+507 345-6790	Avenida Balboa, Torre Ejecutiva	PanamÃ¡	PanamÃ¡	0801	PanamÃ¡	8-234-567890	0987654321	https://www.bebidaspremium.com	Proveedor exclusivo de marcas premium	t	2025-08-04 12:55:31.586848-05	2025-08-05 12:54:52.808252-05	\N	\N
4a8db17f-0c38-4b86-80c7-a23f93c48af8	Carnes Frescas Express	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:55:31.586848-05	\N	\N	Proveedor de carnes frescas y congeladas	Ana MartÃ­nez	ana.martinez@carnesfrescas.com	+507 456-7890	+507 456-7891	Calle 12, Zona Industrial	ColÃ³n	ColÃ³n	0301	PanamÃ¡	8-345-678901	1122334455	https://www.carnesfrescas.com	Carnes de la mÃ¡s alta calidad	t	2025-08-04 12:55:31.586848-05	2025-08-05 12:54:52.808252-05	\N	\N
5f2ac80c-2a87-445f-ae6b-91cd9aed691f	Verduras OrgÃ¡nicas del Valle	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:55:31.586848-05	\N	\N	Productos orgÃ¡nicos y verduras frescas	Luis PÃ©rez	luis.perez@verdurasorganicas.com	+507 567-8901	+507 567-8902	Carretera Interamericana, Km 25	La Chorrera	PanamÃ¡ Oeste	0701	PanamÃ¡	8-456-789012	5544332211	https://www.verdurasorganicas.com	Productos 100% orgÃ¡nicos certificados	t	2025-08-04 12:55:31.586848-05	2025-08-05 12:54:52.808252-05	\N	\N
e2dc0373-cc51-4da8-b6ce-bb9a6b9d3402	Equipos de Cocina Pro	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:55:31.586848-05	\N	\N	Equipos y utensilios profesionales para cocina	Sofia Herrera	sofia.herrera@equiposcocina.com	+507 678-9012	+507 678-9013	Avenida Ricardo J. Alfaro	PanamÃ¡	PanamÃ¡	0801	PanamÃ¡	8-567-890123	6677889900	https://www.equiposcocina.com	Equipos de las mejores marcas	t	2025-08-04 12:55:31.586848-05	2025-08-05 12:54:52.808252-05	\N	\N
e474dcb9-fb56-4239-ad91-d17fca5c2138	Limpieza Industrial Plus	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:55:31.586848-05	\N	\N	Productos de limpieza y desinfecciÃ³n	Roberto Silva	roberto.silva@limpiezaindustrial.com	+507 789-0123	+507 789-0124	Calle 15, Zona Industrial	ArraijÃ¡n	PanamÃ¡ Oeste	0719	PanamÃ¡	8-678-901234	7788990011	https://www.limpiezaindustrial.com	Productos certificados para uso alimentario	t	2025-08-04 12:55:31.586848-05	2025-08-05 12:54:52.808252-05	\N	\N
7242c12b-3d67-4c8c-907f-e6abe4a1ba09	PapelerÃ­a y Oficina Express	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:55:31.586848-05	\N	\N	Suministros de oficina y papelerÃ­a	Carmen Vega	carmen.vega@papeleriaexpress.com	+507 890-1234	+507 890-1235	Calle 23, Centro Comercial	David	ChiriquÃ­	0401	PanamÃ¡	8-789-012345	8899001122	https://www.papeleriaexpress.com	Todo para la oficina	t	2025-08-04 12:55:31.586848-05	2025-08-05 12:54:52.808252-05	\N	\N
25d10760-d248-4f37-a47e-67e826611d77	TecnologÃ­a Avanzada S.A.	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 12:55:31.586848-05	\N	\N	Equipos electrÃ³nicos y tecnologÃ­a	Diego Morales	diego.morales@tecnologiaavanzada.com	+507 901-2345	+507 901-2346	Avenida Central, Plaza TecnolÃ³gica	Santiago	Veraguas	0501	PanamÃ¡	8-890-123456	9900112233	https://www.tecnologiaavanzada.com	TecnologÃ­a de Ãºltima generaciÃ³n	f	2025-08-04 12:55:31.586848-05	2025-08-05 12:54:52.808252-05	\N	\N
00d61dea-283a-434e-b24b-79f699ba7e69	Proveedor Ejemplo	Juan Pérez	juan@proveedor.com	555-1234	\N	Ciudad Ejemplo	\N	\N	\N	\N	\N	\N	t	2025-08-04 14:06:43.863295-05	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	t	2025-08-04 14:06:43.863295-05	2025-08-05 12:54:52.808252-05	\N	\N
e8ec5cd6-a65a-4ec6-8864-9823605e58a6	Distribuidora Central S.A.	\N	maria.gonzalez@distribuidoracentral.com	+507 234-5678	Calle 50, Edificio Plaza Central	Panamá	Panamá	0801	Panamá	8-123-456789	Net 30	7	t	2025-08-04 19:52:18.378058-05	\N	\N	Proveedor principal de productos alimenticios y bebidas	María González	\N	\N	+507 234-5679	\N	\N	\N	\N	\N	\N	1234567890	https://www.distribuidoracentral.com	Proveedor confiable con más de 10 años de experiencia	t	2025-08-04 19:52:18.378058-05	2025-08-05 12:54:52.808252-05	\N	\N
ea0c5667-ba8e-4f90-be56-8054596e7302	Bebidas Premium Ltda.	\N	carlos.rodriguez@bebidaspremium.com	+507 345-6789	Avenida Balboa, Torre Ejecutiva	Panamá	Panamá	0801	Panamá	8-234-567890	Net 15	5	t	2025-08-04 19:52:18.378058-05	\N	\N	Especialistas en bebidas alcohólicas y no alcohólicas	Carlos Rodríguez	\N	\N	+507 345-6790	\N	\N	\N	\N	\N	\N	0987654321	https://www.bebidaspremium.com	Proveedor exclusivo de marcas premium	t	2025-08-04 19:52:18.378058-05	2025-08-05 12:54:52.808252-05	\N	\N
209a88d8-a60d-45c1-b394-33d3a1bb4bbe	Carnes Frescas Express	\N	ana.martinez@carnesfrescas.com	+507 456-7890	Calle 12, Zona Industrial	Colón	Colón	0301	Panamá	8-345-678901	Net 7	3	t	2025-08-04 19:52:18.378058-05	\N	\N	Proveedor de carnes frescas y congeladas	Ana Martínez	\N	\N	+507 456-7891	\N	\N	\N	\N	\N	\N	1122334455	https://www.carnesfrescas.com	Carnes de la más alta calidad	t	2025-08-04 19:52:18.378058-05	2025-08-05 12:54:52.808252-05	\N	\N
3ebfa17b-aa6e-4aff-bf45-0a08085f7b56	Verduras Orgánicas del Valle	\N	luis.perez@verdurasorganicas.com	+507 567-8901	Carretera Interamericana, Km 25	La Chorrera	Panamá Oeste	0701	Panamá	8-456-789012	Net 30	2	t	2025-08-04 19:52:18.378058-05	\N	\N	Productos orgánicos y verduras frescas	Luis Pérez	\N	\N	+507 567-8902	\N	\N	\N	\N	\N	\N	5544332211	https://www.verdurasorganicas.com	Productos 100% orgánicos certificados	t	2025-08-04 19:52:18.378058-05	2025-08-05 12:54:52.808252-05	\N	\N
70141f6a-7415-4ac1-a998-9d20e506283b	Equipos de Cocina Pro	\N	sofia.herrera@equiposcocina.com	+507 678-9012	Avenida Ricardo J. Alfaro	Panamá	Panamá	0801	Panamá	8-567-890123	Net 45	14	t	2025-08-04 19:52:18.378058-05	\N	\N	Equipos y utensilios profesionales para cocina	Sofia Herrera	\N	\N	+507 678-9013	\N	\N	\N	\N	\N	\N	6677889900	https://www.equiposcocina.com	Equipos profesionales de alta calidad	t	2025-08-04 19:52:18.378058-05	2025-08-05 12:54:52.808252-05	\N	\N
\.


--
-- TOC entry 5454 (class 0 OID 175418)
-- Dependencies: 231
-- Data for Name: tables; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tables (id, area_id, table_number, capacity, status, is_active, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5457 (class 0 OID 175487)
-- Dependencies: 234
-- Data for Name: user_assignments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.user_assignments (id, user_id, station_id, area_id, assigned_table_ids, assigned_at, unassigned_at, is_active, notes, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
\.


--
-- TOC entry 5452 (class 0 OID 175386)
-- Dependencies: 229
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, branch_id, "Username", "FirstName", "LastName", full_name, email, password_hash, role, is_active, created_at, company_id, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy") FROM stdin;
770e8400-e29b-41d4-a716-446655440001	660e8400-e29b-41d4-a716-446655440001	admin@restbar.com	Admin	RestBar	Admin RestBar	admin@restbar.com	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	admin	t	2024-01-01 00:00:00-05	550e8400-e29b-41d4-a716-446655440001	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
770e8400-e29b-41d4-a716-446655440004	660e8400-e29b-41d4-a716-446655440006	admin@elsabor.com	Admin	El Sabor	Admin El Sabor	admin@elsabor.com	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	admin	t	2024-01-01 00:00:00-05	550e8400-e29b-41d4-a716-446655440003	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
f98fc35f-1bc1-4ca9-94d6-21b246ef7b86	660e8400-e29b-41d4-a716-446655440001	\N	\N	\N	Irving Isaac Corro Mendoza	icorro@people-inmotion.com	cbYpnyamUPgSDIzEyqdt7JWB8xPSGVCS7yBTGwHDueQ=	waiter	t	2025-07-25 02:31:08.717154-05	\N	2025-08-05 12:54:52.808252-05	2025-08-05 12:54:52.808252-05	\N	\N
770e8400-e29b-41d4-a716-446655440003	660e8400-e29b-41d4-a716-446655440004	admin@cafeexpress.com	Admin	Café Express	Admin Café Express	admin@cafeexpress.com	cbYpnyamUPgSDIzEyqdt7JWB8xPSGVCS7yBTGwHDueQ=	admin	t	2024-01-01 00:00:00-05	550e8400-e29b-41d4-a716-446655440002	2025-08-05 12:54:52.808252-05	2025-08-05 15:40:32.024094-05	\N	admin@restbar.com
\.


--
-- TOC entry 5172 (class 2606 OID 176049)
-- Name: PurchaseOrderItems PK_PurchaseOrderItems; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PurchaseOrderItems"
    ADD CONSTRAINT "PK_PurchaseOrderItems" PRIMARY KEY ("Id");


--
-- TOC entry 5168 (class 2606 OID 176044)
-- Name: PurchaseOrders PK_PurchaseOrders; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PurchaseOrders"
    ADD CONSTRAINT "PK_PurchaseOrders" PRIMARY KEY ("Id");


--
-- TOC entry 5184 (class 2606 OID 176092)
-- Name: TransferItems PK_TransferItems; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TransferItems"
    ADD CONSTRAINT "PK_TransferItems" PRIMARY KEY ("Id");


--
-- TOC entry 5180 (class 2606 OID 176087)
-- Name: Transfers PK_Transfers; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Transfers"
    ADD CONSTRAINT "PK_Transfers" PRIMARY KEY ("Id");


--
-- TOC entry 4957 (class 2606 OID 175213)
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- TOC entry 4962 (class 2606 OID 175267)
-- Name: accounts PK_accounts; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.accounts
    ADD CONSTRAINT "PK_accounts" PRIMARY KEY (id);


--
-- TOC entry 5104 (class 2606 OID 175689)
-- Name: journal_entries PK_journal_entries; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.journal_entries
    ADD CONSTRAINT "PK_journal_entries" PRIMARY KEY (id);


--
-- TOC entry 5111 (class 2606 OID 175718)
-- Name: journal_entry_details PK_journal_entry_details; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.journal_entry_details
    ADD CONSTRAINT "PK_journal_entry_details" PRIMARY KEY (id);


--
-- TOC entry 5014 (class 2606 OID 175412)
-- Name: stations PK_stations; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.stations
    ADD CONSTRAINT "PK_stations" PRIMARY KEY (id);


--
-- TOC entry 5048 (class 2606 OID 175496)
-- Name: user_assignments PK_user_assignments; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_assignments
    ADD CONSTRAINT "PK_user_assignments" PRIMARY KEY (id);


--
-- TOC entry 5002 (class 2606 OID 175380)
-- Name: areas areas_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.areas
    ADD CONSTRAINT areas_pkey PRIMARY KEY (id);


--
-- TOC entry 5032 (class 2606 OID 175439)
-- Name: audit_logs audit_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (id);


--
-- TOC entry 4984 (class 2606 OID 175316)
-- Name: branches branches_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.branches
    ADD CONSTRAINT branches_pkey PRIMARY KEY (id);


--
-- TOC entry 4989 (class 2606 OID 175330)
-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (id);


--
-- TOC entry 4969 (class 2606 OID 175281)
-- Name: companies companies_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.companies
    ADD CONSTRAINT companies_pkey PRIMARY KEY (id);


--
-- TOC entry 4993 (class 2606 OID 175344)
-- Name: company_settings company_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.company_settings
    ADD CONSTRAINT company_settings_pkey PRIMARY KEY (id);


--
-- TOC entry 4997 (class 2606 OID 175362)
-- Name: company_subscriptions company_subscriptions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.company_subscriptions
    ADD CONSTRAINT company_subscriptions_pkey PRIMARY KEY (id);


--
-- TOC entry 4973 (class 2606 OID 175291)
-- Name: customers customers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_pkey PRIMARY KEY (id);


--
-- TOC entry 5116 (class 2606 OID 175791)
-- Name: inventory_histories inventory_histories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_histories
    ADD CONSTRAINT inventory_histories_pkey PRIMARY KEY (id);


--
-- TOC entry 5066 (class 2606 OID 175548)
-- Name: inventory inventory_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory
    ADD CONSTRAINT inventory_pkey PRIMARY KEY (id);


--
-- TOC entry 5075 (class 2606 OID 175585)
-- Name: invoices invoices_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoices
    ADD CONSTRAINT invoices_pkey PRIMARY KEY (id);


--
-- TOC entry 4975 (class 2606 OID 175298)
-- Name: modifiers modifiers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.modifiers
    ADD CONSTRAINT modifiers_pkey PRIMARY KEY (id);


--
-- TOC entry 5080 (class 2606 OID 175605)
-- Name: notifications notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_pkey PRIMARY KEY (id);


--
-- TOC entry 5085 (class 2606 OID 175619)
-- Name: order_cancellation_logs order_cancellation_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_cancellation_logs
    ADD CONSTRAINT order_cancellation_logs_pkey PRIMARY KEY (id);


--
-- TOC entry 5093 (class 2606 OID 175643)
-- Name: order_items order_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_pkey PRIMARY KEY (id);


--
-- TOC entry 5056 (class 2606 OID 175520)
-- Name: orders orders_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (id);


--
-- TOC entry 5098 (class 2606 OID 175674)
-- Name: payments payments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_pkey PRIMARY KEY (id);


--
-- TOC entry 5162 (class 2606 OID 176005)
-- Name: inventory_movements pk_inventory_movements; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_movements
    ADD CONSTRAINT pk_inventory_movements PRIMARY KEY (id);


--
-- TOC entry 4979 (class 2606 OID 175306)
-- Name: product_categories product_categories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product_categories
    ADD CONSTRAINT product_categories_pkey PRIMARY KEY (id);


--
-- TOC entry 5069 (class 2606 OID 175568)
-- Name: product_modifiers product_modifiers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product_modifiers
    ADD CONSTRAINT product_modifiers_pkey PRIMARY KEY (product_id, modifier_id);


--
-- TOC entry 5041 (class 2606 OID 175466)
-- Name: products products_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (id);


--
-- TOC entry 5141 (class 2606 OID 175882)
-- Name: purchase_order_items purchase_order_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_items
    ADD CONSTRAINT purchase_order_items_pkey PRIMARY KEY (id);


--
-- TOC entry 5154 (class 2606 OID 175933)
-- Name: purchase_order_receipt_items purchase_order_receipt_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipt_items
    ADD CONSTRAINT purchase_order_receipt_items_pkey PRIMARY KEY (id);


--
-- TOC entry 5147 (class 2606 OID 175902)
-- Name: purchase_order_receipts purchase_order_receipts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipts
    ADD CONSTRAINT purchase_order_receipts_pkey PRIMARY KEY (id);


--
-- TOC entry 5149 (class 2606 OID 175904)
-- Name: purchase_order_receipts purchase_order_receipts_receipt_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipts
    ADD CONSTRAINT purchase_order_receipts_receipt_number_key UNIQUE (receipt_number);


--
-- TOC entry 5133 (class 2606 OID 175852)
-- Name: purchase_orders purchase_orders_order_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT purchase_orders_order_number_key UNIQUE (order_number);


--
-- TOC entry 5135 (class 2606 OID 175850)
-- Name: purchase_orders purchase_orders_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT purchase_orders_pkey PRIMARY KEY (id);


--
-- TOC entry 5107 (class 2606 OID 175705)
-- Name: split_payments split_payments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.split_payments
    ADD CONSTRAINT split_payments_pkey PRIMARY KEY (id);


--
-- TOC entry 5124 (class 2606 OID 175832)
-- Name: suppliers suppliers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.suppliers
    ADD CONSTRAINT suppliers_pkey PRIMARY KEY (id);


--
-- TOC entry 5019 (class 2606 OID 175425)
-- Name: tables tables_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tables
    ADD CONSTRAINT tables_pkey PRIMARY KEY (id);


--
-- TOC entry 5009 (class 2606 OID 175395)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 5169 (class 1259 OID 176079)
-- Name: IX_PurchaseOrderItems_ProductId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PurchaseOrderItems_ProductId" ON public."PurchaseOrderItems" USING btree ("ProductId");


--
-- TOC entry 5170 (class 1259 OID 176080)
-- Name: IX_PurchaseOrderItems_PurchaseOrderId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PurchaseOrderItems_PurchaseOrderId" ON public."PurchaseOrderItems" USING btree ("PurchaseOrderId");


--
-- TOC entry 5163 (class 1259 OID 176075)
-- Name: IX_PurchaseOrders_BranchId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PurchaseOrders_BranchId" ON public."PurchaseOrders" USING btree ("BranchId");


--
-- TOC entry 5164 (class 1259 OID 176076)
-- Name: IX_PurchaseOrders_CompanyId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PurchaseOrders_CompanyId" ON public."PurchaseOrders" USING btree ("CompanyId");


--
-- TOC entry 5165 (class 1259 OID 176077)
-- Name: IX_PurchaseOrders_CreatedById; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PurchaseOrders_CreatedById" ON public."PurchaseOrders" USING btree ("CreatedById");


--
-- TOC entry 5166 (class 1259 OID 176078)
-- Name: IX_PurchaseOrders_SupplierId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PurchaseOrders_SupplierId" ON public."PurchaseOrders" USING btree ("SupplierId");


--
-- TOC entry 5181 (class 1259 OID 176140)
-- Name: IX_TransferItems_ProductId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_TransferItems_ProductId" ON public."TransferItems" USING btree ("ProductId");


--
-- TOC entry 5182 (class 1259 OID 176139)
-- Name: IX_TransferItems_TransferId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_TransferItems_TransferId" ON public."TransferItems" USING btree ("TransferId");


--
-- TOC entry 5173 (class 1259 OID 176137)
-- Name: IX_Transfers_ApprovedById; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Transfers_ApprovedById" ON public."Transfers" USING btree ("ApprovedById");


--
-- TOC entry 5174 (class 1259 OID 176135)
-- Name: IX_Transfers_CompanyId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Transfers_CompanyId" ON public."Transfers" USING btree ("CompanyId");


--
-- TOC entry 5175 (class 1259 OID 176136)
-- Name: IX_Transfers_CreatedById; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Transfers_CreatedById" ON public."Transfers" USING btree ("CreatedById");


--
-- TOC entry 5176 (class 1259 OID 176134)
-- Name: IX_Transfers_DestinationBranchId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Transfers_DestinationBranchId" ON public."Transfers" USING btree ("DestinationBranchId");


--
-- TOC entry 5177 (class 1259 OID 176138)
-- Name: IX_Transfers_ReceivedById; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Transfers_ReceivedById" ON public."Transfers" USING btree ("ReceivedById");


--
-- TOC entry 5178 (class 1259 OID 176133)
-- Name: IX_Transfers_SourceBranchId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Transfers_SourceBranchId" ON public."Transfers" USING btree ("SourceBranchId");


--
-- TOC entry 4958 (class 1259 OID 176210)
-- Name: IX_accounts_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_accounts_CreatedAt" ON public.accounts USING btree ("CreatedAt");


--
-- TOC entry 4959 (class 1259 OID 176211)
-- Name: IX_accounts_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_accounts_UpdatedAt" ON public.accounts USING btree ("UpdatedAt");


--
-- TOC entry 4960 (class 1259 OID 175729)
-- Name: IX_accounts_parent_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_accounts_parent_account_id" ON public.accounts USING btree (parent_account_id);


--
-- TOC entry 4998 (class 1259 OID 176168)
-- Name: IX_areas_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_areas_CreatedAt" ON public.areas USING btree ("CreatedAt");


--
-- TOC entry 4999 (class 1259 OID 176169)
-- Name: IX_areas_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_areas_UpdatedAt" ON public.areas USING btree ("UpdatedAt");


--
-- TOC entry 5000 (class 1259 OID 175730)
-- Name: IX_areas_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_areas_branch_id" ON public.areas USING btree (branch_id);


--
-- TOC entry 5020 (class 1259 OID 176154)
-- Name: IX_audit_logs_BranchId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_BranchId" ON public.audit_logs USING btree ("BranchId");


--
-- TOC entry 5021 (class 1259 OID 176153)
-- Name: IX_audit_logs_CompanyId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_CompanyId" ON public.audit_logs USING btree ("CompanyId");


--
-- TOC entry 5022 (class 1259 OID 176218)
-- Name: IX_audit_logs_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_CreatedAt" ON public.audit_logs USING btree ("CreatedAt");


--
-- TOC entry 5023 (class 1259 OID 176157)
-- Name: IX_audit_logs_IsError; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_IsError" ON public.audit_logs USING btree ("IsError");


--
-- TOC entry 5024 (class 1259 OID 176155)
-- Name: IX_audit_logs_LogLevel; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_LogLevel" ON public.audit_logs USING btree ("LogLevel");


--
-- TOC entry 5025 (class 1259 OID 176156)
-- Name: IX_audit_logs_Module; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_Module" ON public.audit_logs USING btree ("Module");


--
-- TOC entry 5026 (class 1259 OID 176219)
-- Name: IX_audit_logs_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_UpdatedAt" ON public.audit_logs USING btree ("UpdatedAt");


--
-- TOC entry 5027 (class 1259 OID 175731)
-- Name: IX_audit_logs_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_branch_id" ON public.audit_logs USING btree (branch_id);


--
-- TOC entry 5028 (class 1259 OID 175732)
-- Name: IX_audit_logs_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_company_id" ON public.audit_logs USING btree (company_id);


--
-- TOC entry 5029 (class 1259 OID 176453)
-- Name: IX_audit_logs_timestamp; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_timestamp" ON public.audit_logs USING btree ("timestamp");


--
-- TOC entry 5030 (class 1259 OID 175733)
-- Name: IX_audit_logs_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_audit_logs_user_id" ON public.audit_logs USING btree (user_id);


--
-- TOC entry 4980 (class 1259 OID 176166)
-- Name: IX_branches_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_branches_CreatedAt" ON public.branches USING btree ("CreatedAt");


--
-- TOC entry 4981 (class 1259 OID 176167)
-- Name: IX_branches_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_branches_UpdatedAt" ON public.branches USING btree ("UpdatedAt");


--
-- TOC entry 4982 (class 1259 OID 175734)
-- Name: IX_branches_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_branches_company_id" ON public.branches USING btree (company_id);


--
-- TOC entry 4985 (class 1259 OID 175735)
-- Name: IX_categories_CompanyId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_categories_CompanyId" ON public.categories USING btree ("CompanyId");


--
-- TOC entry 4986 (class 1259 OID 176174)
-- Name: IX_categories_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_categories_CreatedAt" ON public.categories USING btree ("CreatedAt");


--
-- TOC entry 4987 (class 1259 OID 176175)
-- Name: IX_categories_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_categories_UpdatedAt" ON public.categories USING btree ("UpdatedAt");


--
-- TOC entry 4963 (class 1259 OID 176164)
-- Name: IX_companies_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_companies_CreatedAt" ON public.companies USING btree ("CreatedAt");


--
-- TOC entry 4964 (class 1259 OID 176163)
-- Name: IX_companies_IsActive; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_companies_IsActive" ON public.companies USING btree (is_active);


--
-- TOC entry 4965 (class 1259 OID 176165)
-- Name: IX_companies_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_companies_UpdatedAt" ON public.companies USING btree ("UpdatedAt");


--
-- TOC entry 4966 (class 1259 OID 176294)
-- Name: IX_companies_updated_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_companies_updated_at" ON public.companies USING btree (updated_at);


--
-- TOC entry 4990 (class 1259 OID 175738)
-- Name: IX_company_settings_CompanyId1; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_company_settings_CompanyId1" ON public.company_settings USING btree ("CompanyId1");


--
-- TOC entry 4991 (class 1259 OID 175737)
-- Name: IX_company_settings_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_company_settings_company_id" ON public.company_settings USING btree (company_id);


--
-- TOC entry 4994 (class 1259 OID 175740)
-- Name: IX_company_subscriptions_CompanyId1; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_company_subscriptions_CompanyId1" ON public.company_subscriptions USING btree ("CompanyId1");


--
-- TOC entry 4995 (class 1259 OID 175739)
-- Name: IX_company_subscriptions_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_company_subscriptions_company_id" ON public.company_subscriptions USING btree (company_id);


--
-- TOC entry 4970 (class 1259 OID 176206)
-- Name: IX_customers_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_customers_CreatedAt" ON public.customers USING btree ("CreatedAt");


--
-- TOC entry 4971 (class 1259 OID 176207)
-- Name: IX_customers_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_customers_UpdatedAt" ON public.customers USING btree ("UpdatedAt");


--
-- TOC entry 5057 (class 1259 OID 176196)
-- Name: IX_inventory_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_CreatedAt" ON public.inventory USING btree ("CreatedAt");


--
-- TOC entry 5058 (class 1259 OID 176197)
-- Name: IX_inventory_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_UpdatedAt" ON public.inventory USING btree ("UpdatedAt");


--
-- TOC entry 5059 (class 1259 OID 175741)
-- Name: IX_inventory_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_branch_id" ON public.inventory USING btree (branch_id);


--
-- TOC entry 5060 (class 1259 OID 175742)
-- Name: IX_inventory_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_company_id" ON public.inventory USING btree (company_id);


--
-- TOC entry 5155 (class 1259 OID 176198)
-- Name: IX_inventory_movements_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_movements_CreatedAt" ON public.inventory_movements USING btree ("CreatedAt");


--
-- TOC entry 5156 (class 1259 OID 176199)
-- Name: IX_inventory_movements_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_movements_UpdatedAt" ON public.inventory_movements USING btree ("UpdatedAt");


--
-- TOC entry 5157 (class 1259 OID 176006)
-- Name: IX_inventory_movements_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_movements_branch_id" ON public.inventory_movements USING btree (branch_id);


--
-- TOC entry 5158 (class 1259 OID 176007)
-- Name: IX_inventory_movements_inventory_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_movements_inventory_id" ON public.inventory_movements USING btree (inventory_id);


--
-- TOC entry 5159 (class 1259 OID 176008)
-- Name: IX_inventory_movements_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_movements_product_id" ON public.inventory_movements USING btree (product_id);


--
-- TOC entry 5160 (class 1259 OID 176009)
-- Name: IX_inventory_movements_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_movements_user_id" ON public.inventory_movements USING btree (user_id);


--
-- TOC entry 5061 (class 1259 OID 175743)
-- Name: IX_inventory_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_inventory_product_id" ON public.inventory USING btree (product_id);


--
-- TOC entry 5070 (class 1259 OID 176216)
-- Name: IX_invoices_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_invoices_CreatedAt" ON public.invoices USING btree ("CreatedAt");


--
-- TOC entry 5071 (class 1259 OID 176217)
-- Name: IX_invoices_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_invoices_UpdatedAt" ON public.invoices USING btree ("UpdatedAt");


--
-- TOC entry 5072 (class 1259 OID 175744)
-- Name: IX_invoices_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_invoices_customer_id" ON public.invoices USING btree (customer_id);


--
-- TOC entry 5073 (class 1259 OID 175745)
-- Name: IX_invoices_order_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_invoices_order_id" ON public.invoices USING btree (order_id);


--
-- TOC entry 5099 (class 1259 OID 176212)
-- Name: IX_journal_entries_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_journal_entries_CreatedAt" ON public.journal_entries USING btree ("CreatedAt");


--
-- TOC entry 5100 (class 1259 OID 176213)
-- Name: IX_journal_entries_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_journal_entries_UpdatedAt" ON public.journal_entries USING btree ("UpdatedAt");


--
-- TOC entry 5101 (class 1259 OID 175746)
-- Name: IX_journal_entries_order_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_journal_entries_order_id" ON public.journal_entries USING btree (order_id);


--
-- TOC entry 5102 (class 1259 OID 175747)
-- Name: IX_journal_entries_payment_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_journal_entries_payment_id" ON public.journal_entries USING btree (payment_id);


--
-- TOC entry 5108 (class 1259 OID 175748)
-- Name: IX_journal_entry_details_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_journal_entry_details_account_id" ON public.journal_entry_details USING btree (account_id);


--
-- TOC entry 5109 (class 1259 OID 175749)
-- Name: IX_journal_entry_details_journal_entry_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_journal_entry_details_journal_entry_id" ON public.journal_entry_details USING btree (journal_entry_id);


--
-- TOC entry 5076 (class 1259 OID 176208)
-- Name: IX_notifications_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_CreatedAt" ON public.notifications USING btree ("CreatedAt");


--
-- TOC entry 5077 (class 1259 OID 176209)
-- Name: IX_notifications_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_UpdatedAt" ON public.notifications USING btree ("UpdatedAt");


--
-- TOC entry 5078 (class 1259 OID 175750)
-- Name: IX_notifications_order_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_order_id" ON public.notifications USING btree (order_id);


--
-- TOC entry 5081 (class 1259 OID 175751)
-- Name: IX_order_cancellation_logs_order_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_cancellation_logs_order_id" ON public.order_cancellation_logs USING btree (order_id);


--
-- TOC entry 5082 (class 1259 OID 175752)
-- Name: IX_order_cancellation_logs_supervisor_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_cancellation_logs_supervisor_id" ON public.order_cancellation_logs USING btree (supervisor_id);


--
-- TOC entry 5083 (class 1259 OID 175753)
-- Name: IX_order_cancellation_logs_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_cancellation_logs_user_id" ON public.order_cancellation_logs USING btree (user_id);


--
-- TOC entry 5086 (class 1259 OID 176182)
-- Name: IX_order_items_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_items_CreatedAt" ON public.order_items USING btree ("CreatedAt");


--
-- TOC entry 5087 (class 1259 OID 175757)
-- Name: IX_order_items_StationId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_items_StationId" ON public.order_items USING btree ("StationId");


--
-- TOC entry 5088 (class 1259 OID 176183)
-- Name: IX_order_items_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_items_UpdatedAt" ON public.order_items USING btree ("UpdatedAt");


--
-- TOC entry 5089 (class 1259 OID 175754)
-- Name: IX_order_items_order_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_items_order_id" ON public.order_items USING btree (order_id);


--
-- TOC entry 5090 (class 1259 OID 175755)
-- Name: IX_order_items_prepared_by_station_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_items_prepared_by_station_id" ON public.order_items USING btree (prepared_by_station_id);


--
-- TOC entry 5091 (class 1259 OID 175756)
-- Name: IX_order_items_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_order_items_product_id" ON public.order_items USING btree (product_id);


--
-- TOC entry 5049 (class 1259 OID 176180)
-- Name: IX_orders_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_orders_CreatedAt" ON public.orders USING btree ("CreatedAt");


--
-- TOC entry 5050 (class 1259 OID 176181)
-- Name: IX_orders_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_orders_UpdatedAt" ON public.orders USING btree ("UpdatedAt");


--
-- TOC entry 5051 (class 1259 OID 175758)
-- Name: IX_orders_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_orders_company_id" ON public.orders USING btree (company_id);


--
-- TOC entry 5052 (class 1259 OID 175759)
-- Name: IX_orders_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_orders_customer_id" ON public.orders USING btree (customer_id);


--
-- TOC entry 5053 (class 1259 OID 175760)
-- Name: IX_orders_table_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_orders_table_id" ON public.orders USING btree (table_id);


--
-- TOC entry 5054 (class 1259 OID 175761)
-- Name: IX_orders_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_orders_user_id" ON public.orders USING btree (user_id);


--
-- TOC entry 5094 (class 1259 OID 176186)
-- Name: IX_payments_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_payments_CreatedAt" ON public.payments USING btree ("CreatedAt");


--
-- TOC entry 5095 (class 1259 OID 176187)
-- Name: IX_payments_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_payments_UpdatedAt" ON public.payments USING btree ("UpdatedAt");


--
-- TOC entry 5096 (class 1259 OID 175762)
-- Name: IX_payments_order_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_payments_order_id" ON public.payments USING btree (order_id);


--
-- TOC entry 4976 (class 1259 OID 176178)
-- Name: IX_product_categories_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_product_categories_CreatedAt" ON public.product_categories USING btree ("CreatedAt");


--
-- TOC entry 4977 (class 1259 OID 176179)
-- Name: IX_product_categories_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_product_categories_UpdatedAt" ON public.product_categories USING btree ("UpdatedAt");


--
-- TOC entry 5067 (class 1259 OID 175763)
-- Name: IX_product_modifiers_modifier_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_product_modifiers_modifier_id" ON public.product_modifiers USING btree (modifier_id);


--
-- TOC entry 5033 (class 1259 OID 176176)
-- Name: IX_products_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_products_CreatedAt" ON public.products USING btree ("CreatedAt");


--
-- TOC entry 5034 (class 1259 OID 175766)
-- Name: IX_products_ProductCategoryId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_products_ProductCategoryId" ON public.products USING btree ("ProductCategoryId");


--
-- TOC entry 5035 (class 1259 OID 176030)
-- Name: IX_products_SupplierId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_products_SupplierId" ON public.products USING btree ("SupplierId");


--
-- TOC entry 5036 (class 1259 OID 176177)
-- Name: IX_products_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_products_UpdatedAt" ON public.products USING btree ("UpdatedAt");


--
-- TOC entry 5037 (class 1259 OID 175764)
-- Name: IX_products_category_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_products_category_id" ON public.products USING btree (category_id);


--
-- TOC entry 5038 (class 1259 OID 175765)
-- Name: IX_products_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_products_company_id" ON public.products USING btree (company_id);


--
-- TOC entry 5039 (class 1259 OID 175767)
-- Name: IX_products_station_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_products_station_id" ON public.products USING btree (station_id);


--
-- TOC entry 5136 (class 1259 OID 176204)
-- Name: IX_purchase_order_items_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_purchase_order_items_CreatedAt" ON public.purchase_order_items USING btree ("CreatedAt");


--
-- TOC entry 5137 (class 1259 OID 176205)
-- Name: IX_purchase_order_items_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_purchase_order_items_UpdatedAt" ON public.purchase_order_items USING btree ("UpdatedAt");


--
-- TOC entry 5125 (class 1259 OID 176202)
-- Name: IX_purchase_orders_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_purchase_orders_CreatedAt" ON public.purchase_orders USING btree ("CreatedAt");


--
-- TOC entry 5126 (class 1259 OID 176203)
-- Name: IX_purchase_orders_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_purchase_orders_UpdatedAt" ON public.purchase_orders USING btree ("UpdatedAt");


--
-- TOC entry 5105 (class 1259 OID 175768)
-- Name: IX_split_payments_payment_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_split_payments_payment_id" ON public.split_payments USING btree (payment_id);


--
-- TOC entry 5010 (class 1259 OID 176194)
-- Name: IX_stations_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_stations_CreatedAt" ON public.stations USING btree ("CreatedAt");


--
-- TOC entry 5011 (class 1259 OID 176195)
-- Name: IX_stations_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_stations_UpdatedAt" ON public.stations USING btree ("UpdatedAt");


--
-- TOC entry 5012 (class 1259 OID 175769)
-- Name: IX_stations_area_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_stations_area_id" ON public.stations USING btree (area_id);


--
-- TOC entry 5117 (class 1259 OID 176200)
-- Name: IX_suppliers_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_suppliers_CreatedAt" ON public.suppliers USING btree ("CreatedAt");


--
-- TOC entry 5118 (class 1259 OID 176201)
-- Name: IX_suppliers_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_suppliers_UpdatedAt" ON public.suppliers USING btree ("UpdatedAt");


--
-- TOC entry 5015 (class 1259 OID 176172)
-- Name: IX_tables_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_tables_CreatedAt" ON public.tables USING btree ("CreatedAt");


--
-- TOC entry 5016 (class 1259 OID 176173)
-- Name: IX_tables_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_tables_UpdatedAt" ON public.tables USING btree ("UpdatedAt");


--
-- TOC entry 5017 (class 1259 OID 175770)
-- Name: IX_tables_area_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_tables_area_id" ON public.tables USING btree (area_id);


--
-- TOC entry 5042 (class 1259 OID 176190)
-- Name: IX_user_assignments_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_user_assignments_CreatedAt" ON public.user_assignments USING btree ("CreatedAt");


--
-- TOC entry 5043 (class 1259 OID 176191)
-- Name: IX_user_assignments_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_user_assignments_UpdatedAt" ON public.user_assignments USING btree ("UpdatedAt");


--
-- TOC entry 5044 (class 1259 OID 175771)
-- Name: IX_user_assignments_area_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_user_assignments_area_id" ON public.user_assignments USING btree (area_id);


--
-- TOC entry 5045 (class 1259 OID 175772)
-- Name: IX_user_assignments_station_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_user_assignments_station_id" ON public.user_assignments USING btree (station_id);


--
-- TOC entry 5046 (class 1259 OID 175773)
-- Name: IX_user_assignments_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_user_assignments_user_id" ON public.user_assignments USING btree (user_id);


--
-- TOC entry 5003 (class 1259 OID 176188)
-- Name: IX_users_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_users_CreatedAt" ON public.users USING btree ("CreatedAt");


--
-- TOC entry 5004 (class 1259 OID 176189)
-- Name: IX_users_UpdatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_users_UpdatedAt" ON public.users USING btree ("UpdatedAt");


--
-- TOC entry 5005 (class 1259 OID 175774)
-- Name: IX_users_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_users_branch_id" ON public.users USING btree (branch_id);


--
-- TOC entry 5006 (class 1259 OID 175775)
-- Name: IX_users_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_users_company_id" ON public.users USING btree (company_id);


--
-- TOC entry 4967 (class 1259 OID 175736)
-- Name: companies_legal_id_key; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX companies_legal_id_key ON public.companies USING btree (legal_id);


--
-- TOC entry 5062 (class 1259 OID 176486)
-- Name: idx_inventory_expiration_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inventory_expiration_date ON public.inventory USING btree (expiration_date);


--
-- TOC entry 5112 (class 1259 OID 176316)
-- Name: idx_inventory_histories_created_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inventory_histories_created_at ON public.inventory_histories USING btree (created_at);


--
-- TOC entry 5113 (class 1259 OID 175822)
-- Name: idx_inventory_histories_product_branch; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inventory_histories_product_branch ON public.inventory_histories USING btree (product_id, branch_id);


--
-- TOC entry 5114 (class 1259 OID 175820)
-- Name: idx_inventory_histories_type; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inventory_histories_type ON public.inventory_histories USING btree (type);


--
-- TOC entry 5063 (class 1259 OID 175818)
-- Name: idx_inventory_is_perishable; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inventory_is_perishable ON public.inventory USING btree (is_perishable);


--
-- TOC entry 5064 (class 1259 OID 175819)
-- Name: idx_inventory_lot_number; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inventory_lot_number ON public.inventory USING btree (lot_number);


--
-- TOC entry 5138 (class 1259 OID 175955)
-- Name: idx_purchase_order_items_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_items_product_id ON public.purchase_order_items USING btree (product_id);


--
-- TOC entry 5139 (class 1259 OID 175954)
-- Name: idx_purchase_order_items_purchase_order_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_items_purchase_order_id ON public.purchase_order_items USING btree (purchase_order_id);


--
-- TOC entry 5150 (class 1259 OID 175961)
-- Name: idx_purchase_order_receipt_items_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_receipt_items_product_id ON public.purchase_order_receipt_items USING btree (product_id);


--
-- TOC entry 5151 (class 1259 OID 175960)
-- Name: idx_purchase_order_receipt_items_receipt_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_receipt_items_receipt_id ON public.purchase_order_receipt_items USING btree (purchase_order_receipt_id);


--
-- TOC entry 5152 (class 1259 OID 175962)
-- Name: idx_purchase_order_receipt_items_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_receipt_items_status ON public.purchase_order_receipt_items USING btree (status);


--
-- TOC entry 5142 (class 1259 OID 175959)
-- Name: idx_purchase_order_receipts_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_receipts_branch_id ON public.purchase_order_receipts USING btree (branch_id);


--
-- TOC entry 5143 (class 1259 OID 175957)
-- Name: idx_purchase_order_receipts_purchase_order_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_receipts_purchase_order_id ON public.purchase_order_receipts USING btree (purchase_order_id);


--
-- TOC entry 5144 (class 1259 OID 175956)
-- Name: idx_purchase_order_receipts_receipt_number; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_receipts_receipt_number ON public.purchase_order_receipts USING btree (receipt_number);


--
-- TOC entry 5145 (class 1259 OID 175958)
-- Name: idx_purchase_order_receipts_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_order_receipts_status ON public.purchase_order_receipts USING btree (status);


--
-- TOC entry 5127 (class 1259 OID 175952)
-- Name: idx_purchase_orders_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_orders_branch_id ON public.purchase_orders USING btree (branch_id);


--
-- TOC entry 5128 (class 1259 OID 176385)
-- Name: idx_purchase_orders_created_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_orders_created_at ON public.purchase_orders USING btree (created_at);


--
-- TOC entry 5129 (class 1259 OID 175949)
-- Name: idx_purchase_orders_order_number; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_orders_order_number ON public.purchase_orders USING btree (order_number);


--
-- TOC entry 5130 (class 1259 OID 175950)
-- Name: idx_purchase_orders_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_orders_status ON public.purchase_orders USING btree (status);


--
-- TOC entry 5131 (class 1259 OID 175951)
-- Name: idx_purchase_orders_supplier_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchase_orders_supplier_id ON public.purchase_orders USING btree (supplier_id);


--
-- TOC entry 5119 (class 1259 OID 175965)
-- Name: idx_suppliers_company_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_suppliers_company_id ON public.suppliers USING btree (company_id);


--
-- TOC entry 5120 (class 1259 OID 175966)
-- Name: idx_suppliers_is_active; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_suppliers_is_active ON public.suppliers USING btree (is_active);


--
-- TOC entry 5121 (class 1259 OID 175963)
-- Name: idx_suppliers_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_suppliers_name ON public.suppliers USING btree (name);


--
-- TOC entry 5122 (class 1259 OID 175964)
-- Name: idx_suppliers_tax_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_suppliers_tax_id ON public.suppliers USING btree (tax_id);


--
-- TOC entry 5007 (class 1259 OID 175776)
-- Name: users_email_key; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_email_key ON public.users USING btree (email);


--
-- TOC entry 5271 (class 2620 OID 176266)
-- Name: accounts trigger_update_accounts_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_accounts_updated_at BEFORE UPDATE ON public.accounts FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5278 (class 2620 OID 176248)
-- Name: areas trigger_update_areas_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_areas_updated_at BEFORE UPDATE ON public.areas FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5282 (class 2620 OID 176269)
-- Name: audit_logs trigger_update_audit_logs_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_audit_logs_updated_at BEFORE UPDATE ON public.audit_logs FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5276 (class 2620 OID 176247)
-- Name: branches trigger_update_branches_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_branches_updated_at BEFORE UPDATE ON public.branches FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5277 (class 2620 OID 176250)
-- Name: categories trigger_update_categories_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_categories_updated_at BEFORE UPDATE ON public.categories FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5272 (class 2620 OID 176246)
-- Name: companies trigger_update_companies_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_companies_updated_at BEFORE UPDATE ON public.companies FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5274 (class 2620 OID 176264)
-- Name: customers trigger_update_customers_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_customers_updated_at BEFORE UPDATE ON public.customers FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5295 (class 2620 OID 176260)
-- Name: inventory_movements trigger_update_inventory_movements_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_inventory_movements_updated_at BEFORE UPDATE ON public.inventory_movements FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5286 (class 2620 OID 176259)
-- Name: inventory trigger_update_inventory_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_inventory_updated_at BEFORE UPDATE ON public.inventory FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5287 (class 2620 OID 176268)
-- Name: invoices trigger_update_invoices_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_invoices_updated_at BEFORE UPDATE ON public.invoices FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5291 (class 2620 OID 176267)
-- Name: journal_entries trigger_update_journal_entries_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_journal_entries_updated_at BEFORE UPDATE ON public.journal_entries FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5288 (class 2620 OID 176265)
-- Name: notifications trigger_update_notifications_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_notifications_updated_at BEFORE UPDATE ON public.notifications FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5289 (class 2620 OID 176254)
-- Name: order_items trigger_update_order_items_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_order_items_updated_at BEFORE UPDATE ON public.order_items FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5285 (class 2620 OID 176253)
-- Name: orders trigger_update_orders_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_orders_updated_at BEFORE UPDATE ON public.orders FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5290 (class 2620 OID 176255)
-- Name: payments trigger_update_payments_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_payments_updated_at BEFORE UPDATE ON public.payments FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5275 (class 2620 OID 176252)
-- Name: product_categories trigger_update_product_categories_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_product_categories_updated_at BEFORE UPDATE ON public.product_categories FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5283 (class 2620 OID 176251)
-- Name: products trigger_update_products_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_products_updated_at BEFORE UPDATE ON public.products FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5294 (class 2620 OID 176263)
-- Name: purchase_order_items trigger_update_purchase_order_items_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_purchase_order_items_updated_at BEFORE UPDATE ON public.purchase_order_items FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5293 (class 2620 OID 176262)
-- Name: purchase_orders trigger_update_purchase_orders_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_purchase_orders_updated_at BEFORE UPDATE ON public.purchase_orders FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5280 (class 2620 OID 176258)
-- Name: stations trigger_update_stations_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_stations_updated_at BEFORE UPDATE ON public.stations FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5292 (class 2620 OID 176261)
-- Name: suppliers trigger_update_suppliers_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_suppliers_updated_at BEFORE UPDATE ON public.suppliers FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5281 (class 2620 OID 176249)
-- Name: tables trigger_update_tables_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_tables_updated_at BEFORE UPDATE ON public.tables FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5284 (class 2620 OID 176257)
-- Name: user_assignments trigger_update_user_assignments_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_user_assignments_updated_at BEFORE UPDATE ON public.user_assignments FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5279 (class 2620 OID 176256)
-- Name: users trigger_update_users_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_update_users_updated_at BEFORE UPDATE ON public.users FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5273 (class 2620 OID 176161)
-- Name: companies update_companies_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER update_companies_updated_at BEFORE UPDATE ON public.companies FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();


--
-- TOC entry 5261 (class 2606 OID 176065)
-- Name: PurchaseOrderItems FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PurchaseOrderItems"
    ADD CONSTRAINT "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId" FOREIGN KEY ("PurchaseOrderId") REFERENCES public."PurchaseOrders"("Id") ON DELETE CASCADE;


--
-- TOC entry 5262 (class 2606 OID 176070)
-- Name: PurchaseOrderItems FK_PurchaseOrderItems_products_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PurchaseOrderItems"
    ADD CONSTRAINT "FK_PurchaseOrderItems_products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public.products(id) ON DELETE CASCADE;


--
-- TOC entry 5258 (class 2606 OID 176050)
-- Name: PurchaseOrders FK_PurchaseOrders_branches_BranchId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PurchaseOrders"
    ADD CONSTRAINT "FK_PurchaseOrders_branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES public.branches(id) ON DELETE CASCADE;


--
-- TOC entry 5259 (class 2606 OID 176055)
-- Name: PurchaseOrders FK_PurchaseOrders_companies_CompanyId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PurchaseOrders"
    ADD CONSTRAINT "FK_PurchaseOrders_companies_CompanyId" FOREIGN KEY ("CompanyId") REFERENCES public.companies(id) ON DELETE CASCADE;


--
-- TOC entry 5260 (class 2606 OID 176060)
-- Name: PurchaseOrders FK_PurchaseOrders_users_CreatedById; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PurchaseOrders"
    ADD CONSTRAINT "FK_PurchaseOrders_users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES public.users(id) ON DELETE CASCADE;


--
-- TOC entry 5269 (class 2606 OID 176123)
-- Name: TransferItems FK_TransferItems_Transfers_TransferId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TransferItems"
    ADD CONSTRAINT "FK_TransferItems_Transfers_TransferId" FOREIGN KEY ("TransferId") REFERENCES public."Transfers"("Id") ON DELETE CASCADE;


--
-- TOC entry 5270 (class 2606 OID 176128)
-- Name: TransferItems FK_TransferItems_products_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TransferItems"
    ADD CONSTRAINT "FK_TransferItems_products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public.products(id) ON DELETE CASCADE;


--
-- TOC entry 5263 (class 2606 OID 176098)
-- Name: Transfers FK_Transfers_branches_DestinationBranchId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Transfers"
    ADD CONSTRAINT "FK_Transfers_branches_DestinationBranchId" FOREIGN KEY ("DestinationBranchId") REFERENCES public.branches(id) ON DELETE CASCADE;


--
-- TOC entry 5264 (class 2606 OID 176093)
-- Name: Transfers FK_Transfers_branches_SourceBranchId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Transfers"
    ADD CONSTRAINT "FK_Transfers_branches_SourceBranchId" FOREIGN KEY ("SourceBranchId") REFERENCES public.branches(id) ON DELETE CASCADE;


--
-- TOC entry 5265 (class 2606 OID 176103)
-- Name: Transfers FK_Transfers_companies_CompanyId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Transfers"
    ADD CONSTRAINT "FK_Transfers_companies_CompanyId" FOREIGN KEY ("CompanyId") REFERENCES public.companies(id) ON DELETE CASCADE;


--
-- TOC entry 5266 (class 2606 OID 176113)
-- Name: Transfers FK_Transfers_users_ApprovedById; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Transfers"
    ADD CONSTRAINT "FK_Transfers_users_ApprovedById" FOREIGN KEY ("ApprovedById") REFERENCES public.users(id) ON DELETE SET NULL;


--
-- TOC entry 5267 (class 2606 OID 176108)
-- Name: Transfers FK_Transfers_users_CreatedById; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Transfers"
    ADD CONSTRAINT "FK_Transfers_users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES public.users(id) ON DELETE CASCADE;


--
-- TOC entry 5268 (class 2606 OID 176118)
-- Name: Transfers FK_Transfers_users_ReceivedById; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Transfers"
    ADD CONSTRAINT "FK_Transfers_users_ReceivedById" FOREIGN KEY ("ReceivedById") REFERENCES public.users(id) ON DELETE SET NULL;


--
-- TOC entry 5185 (class 2606 OID 175268)
-- Name: accounts FK_accounts_accounts_parent_account_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.accounts
    ADD CONSTRAINT "FK_accounts_accounts_parent_account_id" FOREIGN KEY (parent_account_id) REFERENCES public.accounts(id) ON DELETE RESTRICT;


--
-- TOC entry 5197 (class 2606 OID 176148)
-- Name: audit_logs FK_audit_logs_branches_BranchId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT "FK_audit_logs_branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES public.branches(id);


--
-- TOC entry 5198 (class 2606 OID 176143)
-- Name: audit_logs FK_audit_logs_companies_CompanyId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT "FK_audit_logs_companies_CompanyId" FOREIGN KEY ("CompanyId") REFERENCES public.companies(id);


--
-- TOC entry 5187 (class 2606 OID 175331)
-- Name: categories FK_categories_companies_CompanyId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT "FK_categories_companies_CompanyId" FOREIGN KEY ("CompanyId") REFERENCES public.companies(id);


--
-- TOC entry 5188 (class 2606 OID 175345)
-- Name: company_settings FK_company_settings_companies_CompanyId1; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.company_settings
    ADD CONSTRAINT "FK_company_settings_companies_CompanyId1" FOREIGN KEY ("CompanyId1") REFERENCES public.companies(id);


--
-- TOC entry 5190 (class 2606 OID 175363)
-- Name: company_subscriptions FK_company_subscriptions_companies_CompanyId1; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.company_subscriptions
    ADD CONSTRAINT "FK_company_subscriptions_companies_CompanyId1" FOREIGN KEY ("CompanyId1") REFERENCES public.companies(id);


--
-- TOC entry 5230 (class 2606 OID 175690)
-- Name: journal_entries FK_journal_entries_orders_order_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.journal_entries
    ADD CONSTRAINT "FK_journal_entries_orders_order_id" FOREIGN KEY (order_id) REFERENCES public.orders(id) ON DELETE SET NULL;


--
-- TOC entry 5231 (class 2606 OID 175695)
-- Name: journal_entries FK_journal_entries_payments_payment_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.journal_entries
    ADD CONSTRAINT "FK_journal_entries_payments_payment_id" FOREIGN KEY (payment_id) REFERENCES public.payments(id) ON DELETE SET NULL;


--
-- TOC entry 5233 (class 2606 OID 175719)
-- Name: journal_entry_details FK_journal_entry_details_accounts_account_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.journal_entry_details
    ADD CONSTRAINT "FK_journal_entry_details_accounts_account_id" FOREIGN KEY (account_id) REFERENCES public.accounts(id) ON DELETE RESTRICT;


--
-- TOC entry 5234 (class 2606 OID 175724)
-- Name: journal_entry_details FK_journal_entry_details_journal_entries_journal_entry_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.journal_entry_details
    ADD CONSTRAINT "FK_journal_entry_details_journal_entries_journal_entry_id" FOREIGN KEY (journal_entry_id) REFERENCES public.journal_entries(id) ON DELETE CASCADE;


--
-- TOC entry 5225 (class 2606 OID 175644)
-- Name: order_items FK_order_items_stations_StationId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT "FK_order_items_stations_StationId" FOREIGN KEY ("StationId") REFERENCES public.stations(id);


--
-- TOC entry 5202 (class 2606 OID 175467)
-- Name: products FK_products_product_categories_ProductCategoryId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_product_categories_ProductCategoryId" FOREIGN KEY ("ProductCategoryId") REFERENCES public.product_categories(id);


--
-- TOC entry 5203 (class 2606 OID 176033)
-- Name: products FK_products_suppliers_SupplierId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES public.suppliers(id) ON DELETE SET NULL;


--
-- TOC entry 5195 (class 2606 OID 175413)
-- Name: stations FK_stations_areas_area_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.stations
    ADD CONSTRAINT "FK_stations_areas_area_id" FOREIGN KEY (area_id) REFERENCES public.areas(id) ON DELETE SET NULL;


--
-- TOC entry 5192 (class 2606 OID 175381)
-- Name: areas areas_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.areas
    ADD CONSTRAINT areas_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(id);


--
-- TOC entry 5199 (class 2606 OID 175440)
-- Name: audit_logs audit_logs_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(id) ON DELETE SET NULL;


--
-- TOC entry 5200 (class 2606 OID 175445)
-- Name: audit_logs audit_logs_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE SET NULL;


--
-- TOC entry 5201 (class 2606 OID 175450)
-- Name: audit_logs audit_logs_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- TOC entry 5186 (class 2606 OID 175317)
-- Name: branches branches_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.branches
    ADD CONSTRAINT branches_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id);


--
-- TOC entry 5189 (class 2606 OID 175350)
-- Name: company_settings company_settings_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.company_settings
    ADD CONSTRAINT company_settings_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE CASCADE;


--
-- TOC entry 5191 (class 2606 OID 175368)
-- Name: company_subscriptions company_subscriptions_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.company_subscriptions
    ADD CONSTRAINT company_subscriptions_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE CASCADE;


--
-- TOC entry 5254 (class 2606 OID 176010)
-- Name: inventory_movements fk_inventory_movements_branches_branch_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_movements
    ADD CONSTRAINT fk_inventory_movements_branches_branch_id FOREIGN KEY (branch_id) REFERENCES public.branches(id) ON DELETE RESTRICT;


--
-- TOC entry 5255 (class 2606 OID 176015)
-- Name: inventory_movements fk_inventory_movements_inventory_inventory_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_movements
    ADD CONSTRAINT fk_inventory_movements_inventory_inventory_id FOREIGN KEY (inventory_id) REFERENCES public.inventory(id) ON DELETE CASCADE;


--
-- TOC entry 5256 (class 2606 OID 176020)
-- Name: inventory_movements fk_inventory_movements_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_movements
    ADD CONSTRAINT fk_inventory_movements_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- TOC entry 5257 (class 2606 OID 176025)
-- Name: inventory_movements fk_inventory_movements_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_movements
    ADD CONSTRAINT fk_inventory_movements_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- TOC entry 5214 (class 2606 OID 175549)
-- Name: inventory inventory_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory
    ADD CONSTRAINT inventory_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(id);


--
-- TOC entry 5215 (class 2606 OID 175554)
-- Name: inventory inventory_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory
    ADD CONSTRAINT inventory_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE SET NULL;


--
-- TOC entry 5235 (class 2606 OID 175802)
-- Name: inventory_histories inventory_histories_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_histories
    ADD CONSTRAINT inventory_histories_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(id) ON DELETE CASCADE;


--
-- TOC entry 5236 (class 2606 OID 175807)
-- Name: inventory_histories inventory_histories_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_histories
    ADD CONSTRAINT inventory_histories_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE CASCADE;


--
-- TOC entry 5237 (class 2606 OID 175812)
-- Name: inventory_histories inventory_histories_created_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_histories
    ADD CONSTRAINT inventory_histories_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- TOC entry 5238 (class 2606 OID 175792)
-- Name: inventory_histories inventory_histories_inventory_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_histories
    ADD CONSTRAINT inventory_histories_inventory_id_fkey FOREIGN KEY (inventory_id) REFERENCES public.inventory(id) ON DELETE CASCADE;


--
-- TOC entry 5239 (class 2606 OID 175797)
-- Name: inventory_histories inventory_histories_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory_histories
    ADD CONSTRAINT inventory_histories_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- TOC entry 5216 (class 2606 OID 175559)
-- Name: inventory inventory_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inventory
    ADD CONSTRAINT inventory_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id);


--
-- TOC entry 5219 (class 2606 OID 175586)
-- Name: invoices invoices_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoices
    ADD CONSTRAINT invoices_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customers(id);


--
-- TOC entry 5220 (class 2606 OID 175591)
-- Name: invoices invoices_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoices
    ADD CONSTRAINT invoices_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(id);


--
-- TOC entry 5221 (class 2606 OID 175606)
-- Name: notifications notifications_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(id);


--
-- TOC entry 5222 (class 2606 OID 175620)
-- Name: order_cancellation_logs order_cancellation_logs_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_cancellation_logs
    ADD CONSTRAINT order_cancellation_logs_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(id) ON DELETE CASCADE;


--
-- TOC entry 5223 (class 2606 OID 175625)
-- Name: order_cancellation_logs order_cancellation_logs_supervisor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_cancellation_logs
    ADD CONSTRAINT order_cancellation_logs_supervisor_id_fkey FOREIGN KEY (supervisor_id) REFERENCES public.users(id);


--
-- TOC entry 5224 (class 2606 OID 175630)
-- Name: order_cancellation_logs order_cancellation_logs_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_cancellation_logs
    ADD CONSTRAINT order_cancellation_logs_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- TOC entry 5226 (class 2606 OID 175649)
-- Name: order_items order_items_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(id);


--
-- TOC entry 5227 (class 2606 OID 175654)
-- Name: order_items order_items_prepared_by_station_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_prepared_by_station_id_fkey FOREIGN KEY (prepared_by_station_id) REFERENCES public.stations(id);


--
-- TOC entry 5228 (class 2606 OID 175659)
-- Name: order_items order_items_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id);


--
-- TOC entry 5210 (class 2606 OID 175521)
-- Name: orders orders_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE SET NULL;


--
-- TOC entry 5211 (class 2606 OID 175526)
-- Name: orders orders_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customers(id);


--
-- TOC entry 5212 (class 2606 OID 175531)
-- Name: orders orders_table_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_table_id_fkey FOREIGN KEY (table_id) REFERENCES public.tables(id);


--
-- TOC entry 5213 (class 2606 OID 175536)
-- Name: orders orders_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- TOC entry 5229 (class 2606 OID 175675)
-- Name: payments payments_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(id);


--
-- TOC entry 5217 (class 2606 OID 175569)
-- Name: product_modifiers product_modifiers_modifier_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product_modifiers
    ADD CONSTRAINT product_modifiers_modifier_id_fkey FOREIGN KEY (modifier_id) REFERENCES public.modifiers(id);


--
-- TOC entry 5218 (class 2606 OID 175574)
-- Name: product_modifiers product_modifiers_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product_modifiers
    ADD CONSTRAINT product_modifiers_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id);


--
-- TOC entry 5204 (class 2606 OID 175472)
-- Name: products products_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.categories(id) ON DELETE SET NULL;


--
-- TOC entry 5205 (class 2606 OID 175477)
-- Name: products products_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE SET NULL;


--
-- TOC entry 5206 (class 2606 OID 175482)
-- Name: products products_station_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_station_id_fkey FOREIGN KEY (station_id) REFERENCES public.stations(id) ON DELETE SET NULL;


--
-- TOC entry 5245 (class 2606 OID 175888)
-- Name: purchase_order_items purchase_order_items_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_items
    ADD CONSTRAINT purchase_order_items_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE SET NULL;


--
-- TOC entry 5246 (class 2606 OID 175883)
-- Name: purchase_order_items purchase_order_items_purchase_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_items
    ADD CONSTRAINT purchase_order_items_purchase_order_id_fkey FOREIGN KEY (purchase_order_id) REFERENCES public.purchase_orders(id) ON DELETE CASCADE;


--
-- TOC entry 5251 (class 2606 OID 175944)
-- Name: purchase_order_receipt_items purchase_order_receipt_items_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipt_items
    ADD CONSTRAINT purchase_order_receipt_items_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE SET NULL;


--
-- TOC entry 5252 (class 2606 OID 175939)
-- Name: purchase_order_receipt_items purchase_order_receipt_items_purchase_order_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipt_items
    ADD CONSTRAINT purchase_order_receipt_items_purchase_order_item_id_fkey FOREIGN KEY (purchase_order_item_id) REFERENCES public.purchase_order_items(id) ON DELETE SET NULL;


--
-- TOC entry 5253 (class 2606 OID 175934)
-- Name: purchase_order_receipt_items purchase_order_receipt_items_purchase_order_receipt_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipt_items
    ADD CONSTRAINT purchase_order_receipt_items_purchase_order_receipt_id_fkey FOREIGN KEY (purchase_order_receipt_id) REFERENCES public.purchase_order_receipts(id) ON DELETE CASCADE;


--
-- TOC entry 5247 (class 2606 OID 175915)
-- Name: purchase_order_receipts purchase_order_receipts_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipts
    ADD CONSTRAINT purchase_order_receipts_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(id) ON DELETE CASCADE;


--
-- TOC entry 5248 (class 2606 OID 175920)
-- Name: purchase_order_receipts purchase_order_receipts_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipts
    ADD CONSTRAINT purchase_order_receipts_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE CASCADE;


--
-- TOC entry 5249 (class 2606 OID 175905)
-- Name: purchase_order_receipts purchase_order_receipts_purchase_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipts
    ADD CONSTRAINT purchase_order_receipts_purchase_order_id_fkey FOREIGN KEY (purchase_order_id) REFERENCES public.purchase_orders(id) ON DELETE CASCADE;


--
-- TOC entry 5250 (class 2606 OID 175910)
-- Name: purchase_order_receipts purchase_order_receipts_received_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_order_receipts
    ADD CONSTRAINT purchase_order_receipts_received_by_user_id_fkey FOREIGN KEY (received_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- TOC entry 5241 (class 2606 OID 175853)
-- Name: purchase_orders purchase_orders_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT purchase_orders_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(id) ON DELETE CASCADE;


--
-- TOC entry 5242 (class 2606 OID 175858)
-- Name: purchase_orders purchase_orders_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT purchase_orders_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE CASCADE;


--
-- TOC entry 5243 (class 2606 OID 175868)
-- Name: purchase_orders purchase_orders_created_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT purchase_orders_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- TOC entry 5244 (class 2606 OID 175863)
-- Name: purchase_orders purchase_orders_supplier_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT purchase_orders_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.suppliers(id) ON DELETE SET NULL;


--
-- TOC entry 5232 (class 2606 OID 175706)
-- Name: split_payments split_payments_payment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.split_payments
    ADD CONSTRAINT split_payments_payment_id_fkey FOREIGN KEY (payment_id) REFERENCES public.payments(id);


--
-- TOC entry 5240 (class 2606 OID 175833)
-- Name: suppliers suppliers_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.suppliers
    ADD CONSTRAINT suppliers_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE CASCADE;


--
-- TOC entry 5196 (class 2606 OID 175426)
-- Name: tables tables_area_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tables
    ADD CONSTRAINT tables_area_id_fkey FOREIGN KEY (area_id) REFERENCES public.areas(id);


--
-- TOC entry 5207 (class 2606 OID 175497)
-- Name: user_assignments user_assignments_area_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_assignments
    ADD CONSTRAINT user_assignments_area_id_fkey FOREIGN KEY (area_id) REFERENCES public.areas(id);


--
-- TOC entry 5208 (class 2606 OID 175502)
-- Name: user_assignments user_assignments_station_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_assignments
    ADD CONSTRAINT user_assignments_station_id_fkey FOREIGN KEY (station_id) REFERENCES public.stations(id);


--
-- TOC entry 5209 (class 2606 OID 175507)
-- Name: user_assignments user_assignments_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_assignments
    ADD CONSTRAINT user_assignments_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- TOC entry 5193 (class 2606 OID 175396)
-- Name: users users_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(id);


--
-- TOC entry 5194 (class 2606 OID 175401)
-- Name: users users_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id) ON DELETE SET NULL;


-- Completed on 2025-08-05 17:35:37

--
-- PostgreSQL database dump complete
--

